
using PDFtoImage;
using SkiaSharp;
using System.Runtime.Versioning;
using Marilog.Infrastructure.Interfaces.OCR;

namespace Marilog.Infrastructure.Services.OCR
{
    [SupportedOSPlatform("windows")]
    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("macos")]
    public sealed class PdfThumbnailGenerator : IThumbnailGenerator
    {
        public bool CanGenerate(string contentType) =>
         string.Equals(contentType, "application/pdf", StringComparison.OrdinalIgnoreCase);

        public async Task<string?> GenerateAsync(
            string sourceFullPath,
            CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            return await Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();

                string thumbnailPath = Path.ChangeExtension(sourceFullPath, ".png");

                const int maxSize = 300;

                using var pdfStream = File.OpenRead(sourceFullPath);

                using var original = Conversion.ToImage(
                    pdfStream,
                    page: 0,
                    options: new RenderOptions(Dpi: 200));

                float scale = Math.Min(
                    (float)maxSize / original.Width,
                    (float)maxSize / original.Height);

                scale = Math.Min(scale, 1f);

                int width = Math.Max(1, (int)Math.Round(original.Width * scale));
                int height = Math.Max(1, (int)Math.Round(original.Height * scale));

                using var thumbnail = original.Resize(
                    new SKImageInfo(width, height),
                    new SKSamplingOptions(SKCubicResampler.CatmullRom));

                using var image = SKImage.FromBitmap(thumbnail);

                using var data = image.Encode(
                    SKEncodedImageFormat.Png,
                    100);

                using var output = File.Create(thumbnailPath);

                data.SaveTo(output);

                return thumbnailPath;

            }, ct);
        }
    }
}
