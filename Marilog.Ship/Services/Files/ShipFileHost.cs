using Marilog.Contracts.Interfaces.Services.SystemServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Microsoft.Web.WebView2.Core;
using System.Diagnostics;
using System.IO;

namespace Marilog.Ship.Services.Files
{
    /// <summary>
    /// Takes the place of Marilog's StoredFilesController inside the ship window.
    /// The WebView2 answers {BaseUrl}api/StoredFiles/{id}/stream and /thumbnailStream itself,
    /// with the controller's rules, so ManageStoredFilesDialog works unchanged
    /// (thumbnails, preview panel, Office previews).
    /// "Download" and "Open in new tab" open the file in its Windows program instead (index.html sends them here).
    /// </summary>
    public static class ShipFileHost
    {
        public const string BaseUrl = "https://files.ship.example/";

        // Same list as StoredFilesController.InlineSafeTypes.
        private static readonly HashSet<string> InlineSafeTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "application/pdf", "image/png", "image/jpeg", "image/gif", "image/bmp", "image/webp", "text/plain"
        };

        // Same as the controller: the HTML preview is built with every value encoded; the sandbox is a second guard.
        private const string PreviewPolicy = "Content-Security-Policy: sandbox; default-src 'none'; style-src 'unsafe-inline'";

        private const string NoPreviewHtml =
            "<p style=\"font-family:sans-serif;color:#666;margin:16px\">No preview yet. " +
            "The file may still be processing, or it has no preview. Use Download.</p>";

        private static IServiceScopeFactory? _scopeFactory;

        public static string DownloadUrl(int fileId) => $"{BaseUrl}api/StoredFiles/{fileId}/stream";

        public static string ThumbnailUrl(int fileId) => $"{BaseUrl}api/StoredFiles/{fileId}/thumbnailStream";

        public static void Attach(CoreWebView2 webView, IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            webView.AddWebResourceRequestedFilter(BaseUrl + "*", CoreWebView2WebResourceContext.All);
            webView.WebResourceRequested += OnWebResourceRequested;
        }

        // ── Serving (stream / thumbnail / preview) ───────────────────────────────

        private static async void OnWebResourceRequested(object? sender, CoreWebView2WebResourceRequestedEventArgs e)
        {
            if (!e.Request.Uri.StartsWith(BaseUrl, StringComparison.OrdinalIgnoreCase))
                return;   // the app's own requests

            var webView = (CoreWebView2)sender!;
            var deferral = e.GetDeferral();
            try
            {
                var reply = await BuildReplyAsync(new Uri(e.Request.Uri));
                e.Response = webView.Environment.CreateWebResourceResponse(
                    reply.Content, reply.Status, reply.Status == 200 ? "OK" : "Not Found", reply.Headers);
            }
            catch (Exception)
            {
                e.Response = webView.Environment.CreateWebResourceResponse(null, 500, "Error", string.Empty);
            }
            finally
            {
                deferral.Complete();
            }
        }

        private sealed record Reply(int Status, Stream? Content, string Headers);

        private static async Task<Reply> BuildReplyAsync(Uri uri)
        {
            // api / StoredFiles / {id} / stream | thumbnailStream
            var parts = uri.AbsolutePath.Trim('/').Split('/');
            if (parts.Length != 4 || !int.TryParse(parts[2], out var id))
                return NotFound();

            var inline = uri.Query.Contains("inline=true", StringComparison.OrdinalIgnoreCase);

            await using var scope = _scopeFactory!.CreateAsyncScope();
            var files = scope.ServiceProvider.GetRequiredService<IStoredFileService>();

            if (parts[3].Equals("thumbnailStream", StringComparison.OrdinalIgnoreCase))
            {
                var thumbnail = await files.GetThumbnailStreamAsync(id);
                return thumbnail is null ? NotFound() : Ok(await CopyAsync(thumbnail), "image/png");
            }

            if (!parts[3].Equals("stream", StringComparison.OrdinalIgnoreCase))
                return NotFound();

            var file = await files.GetByIdAsync(id);
            if (file is null)
                return NotFound();

            var isSafe = InlineSafeTypes.Contains(file.ContentType);
            var fileName = Uri.EscapeDataString(file.OriginalFileName);

            if (inline && isSafe)
                return Ok(await CopyAsync(await files.GetFileStreamAsync(id)), file.ContentType,
                    $"Content-Disposition: inline; filename*=UTF-8''{fileName}");

            // Word/Excel/PowerPoint: the HTML copy made next to the file.
            if (inline && await files.GetPreviewStreamAsync(id) is { } preview)
                return Ok(await CopyAsync(preview), "text/html; charset=utf-8", PreviewPolicy);

            if (inline)
                return Ok(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(NoPreviewHtml)),
                    "text/html; charset=utf-8", PreviewPolicy);

            return Ok(await CopyAsync(await files.GetFileStreamAsync(id)),
                isSafe ? file.ContentType : "application/octet-stream",
                $"Content-Disposition: attachment; filename*=UTF-8''{fileName}");
        }

        private static Reply Ok(Stream content, string contentType, string? extraHeader = null) =>
            new(200, content,
                $"Content-Type: {contentType}\r\nX-Content-Type-Options: nosniff" +
                (extraHeader is null ? string.Empty : "\r\n" + extraHeader));

        private static Reply NotFound() => new(404, null, string.Empty);

        // A copy in memory: the file on disk is not kept open by the WebView (it can still be deleted).
        private static async Task<Stream> CopyAsync(Stream source)
        {
            var copy = new MemoryStream();
            await using (source)
                await source.CopyToAsync(copy);
            copy.Position = 0;
            return copy;
        }

        // ── Download / open in new tab ───────────────────────────────────────────

        /// <summary>
        /// Called from index.html instead of window.open for a file URL: a temporary copy of the file
        /// is opened in its Windows program (Acrobat, Excel, Photos ...). The stored original is never touched.
        /// </summary>
        [JSInvokable]
        public static async Task OpenStoredFile(string url)
        {
            var parts = new Uri(url).AbsolutePath.Trim('/').Split('/');
            if (parts.Length != 4 || !int.TryParse(parts[2], out var id))
                return;

            await using var scope = _scopeFactory!.CreateAsyncScope();
            var files = scope.ServiceProvider.GetRequiredService<IStoredFileService>();

            var file = await files.GetByIdAsync(id);
            if (file is null)
                return;

            var folder = Path.Combine(Path.GetTempPath(), "Marilog Ship", id.ToString());
            Directory.CreateDirectory(folder);

            var invalid = Path.GetInvalidFileNameChars();
            var name = string.Concat(file.OriginalFileName.Select(c => invalid.Contains(c) ? '_' : c));
            var path = Path.Combine(folder, name);

            try
            {
                await using var source = await files.GetFileStreamAsync(id);
                await using var target = File.Create(path);
                await source.CopyToAsync(target);
            }
            catch (IOException) when (File.Exists(path))
            {
                // The copy is already open in its program — open it again as it is.
            }

            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }
    }
}