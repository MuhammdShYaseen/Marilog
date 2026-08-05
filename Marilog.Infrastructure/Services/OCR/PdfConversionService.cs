using Marilog.Infrastructure.Interfaces.OCR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Marilog.Infrastructure.Services.OCR
{
    public class PdfConversionService : IPdfConversionService
    {
        private readonly ILogger<PdfConversionService> _logger;
        private readonly string _libreOfficePath;
        private readonly TimeSpan _conversionTimeout;

        public PdfConversionService(ILogger<PdfConversionService> logger, IConfiguration configuration)
        {
            _logger = logger;

            // Allow override via appsettings ("LibreOffice:ExecutablePath"),
            // otherwise fall back to OS-specific defaults.
            _libreOfficePath = configuration["LibreOffice:ExecutablePath"]
                ?? ResolveDefaultLibreOfficePath();

            var timeoutSeconds = configuration.GetValue<int?>("LibreOffice:TimeoutSeconds") ?? 90;
            _conversionTimeout = TimeSpan.FromSeconds(timeoutSeconds);
        }

        /// <summary>
        /// Ensures the file at <paramref name="filePath"/> is a PDF, converting it if needed.
        /// Returns the resulting file's path — this will DIFFER from the input path
        /// whenever a conversion happened (extension changes to ".pdf"), so callers
        /// MUST use the returned path (and update any DB record such as StoredFile)
        /// instead of assuming the original path still applies.
        /// Returns null if conversion failed to produce an output file.
        /// </summary>
        public async Task<string?> EnsurePdfAsync(string filePath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path is required.", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found.", filePath);

            var extension = Path.GetExtension(filePath);

            // Already PDF
            if (extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                return filePath;

            var outputPdf = Path.ChangeExtension(filePath, ".pdf");

            try
            {
                await ConvertToPdfAsync(filePath, outputPdf, cancellationToken);

                if (!File.Exists(outputPdf))
                    return null;

                // Conversion succeeded: outputPdf already has the correct ".pdf"
                // extension. Remove the original (e.g. .txt/.docx) file rather than
                // overwriting it, so the file on disk always matches its extension.
                TryDelete(filePath);

                return outputPdf;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed converting {File} to PDF.", filePath);

                // Clean up a half-written output file, if any.
                TryDelete(outputPdf);

                throw new InvalidOperationException(
                    $"Failed to convert '{Path.GetFileName(filePath)}' to PDF.",
                    ex);
            }
        }

        private async Task ConvertToPdfAsync(string filePath, string outputPdf, CancellationToken cancellationToken)
        {
            if (!File.Exists(_libreOfficePath))
                throw new FileNotFoundException("LibreOffice executable not found.", _libreOfficePath);

            var outputDirectory = Path.GetDirectoryName(outputPdf);

            if (string.IsNullOrWhiteSpace(outputDirectory))
                throw new InvalidOperationException("Output directory is invalid.");

            // Each conversion gets its own LibreOffice user profile so that
            // concurrent conversions (e.g. multiple attachments processed in
            // parallel by the background worker) don't collide on the same
            // profile lock and fail randomly.
            var profileDir = Path.Combine(Path.GetTempPath(), "lo_profile_" + Guid.NewGuid().ToString("N"));
            var profileUri = new Uri(profileDir).AbsoluteUri;

            var startInfo = new ProcessStartInfo
            {
                FileName = _libreOfficePath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            // ArgumentList avoids manual quoting bugs (paths with spaces/quotes).
            startInfo.ArgumentList.Add("--headless");
            startInfo.ArgumentList.Add("--norestore");
            startInfo.ArgumentList.Add($"-env:UserInstallation={profileUri}");
            startInfo.ArgumentList.Add("--convert-to");
            startInfo.ArgumentList.Add("pdf");
            startInfo.ArgumentList.Add("--outdir");
            startInfo.ArgumentList.Add(outputDirectory);
            startInfo.ArgumentList.Add(filePath);

            using var process = new Process { StartInfo = startInfo };
            using var timeoutCts = new CancellationTokenSource(_conversionTimeout);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            try
            {
                process.Start();

                // Read both streams concurrently to avoid a pipe-buffer deadlock
                // if one stream fills up while we're still blocked reading the other.
                var errorTask = process.StandardError.ReadToEndAsync(linkedCts.Token);
                var outputTask = process.StandardOutput.ReadToEndAsync(linkedCts.Token);

                await process.WaitForExitAsync(linkedCts.Token);
                var error = await errorTask;
                var output = await outputTask;

                if (process.ExitCode != 0)
                    throw new InvalidOperationException($"LibreOffice conversion failed: {error}");

                if (!File.Exists(outputPdf))
                    throw new InvalidOperationException($"PDF conversion failed. LibreOffice output: {output}");
            }
            catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
            {
                throw new TimeoutException(
                    $"LibreOffice conversion of '{Path.GetFileName(filePath)}' did not complete within {_conversionTimeout.TotalSeconds}s.");
            }
            finally
            {
                if (!process.HasExited)
                {
                    try { process.Kill(entireProcessTree: true); }
                    catch { /* best effort */ }
                }

                TryDeleteDirectory(profileDir);
            }
        }

        private static string ResolveDefaultLibreOfficePath()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return @"C:\Program Files\LibreOffice\program\soffice.exe";

            // Common locations on Linux (adjust if your VPS installs elsewhere).
            var candidates = new[]
            {
                "/usr/bin/soffice",
                "/usr/bin/libreoffice",
                "/opt/libreoffice/program/soffice"
            };

            return candidates.FirstOrDefault(File.Exists) ?? candidates[0];
        }

        private void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to clean up temp file {Path}.", path);
            }
        }

        private void TryDeleteDirectory(string path)
        {
            try
            {
                if (Directory.Exists(path))
                    Directory.Delete(path, recursive: true);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to clean up LibreOffice profile dir {Path}.", path);
            }
        }
    }
}