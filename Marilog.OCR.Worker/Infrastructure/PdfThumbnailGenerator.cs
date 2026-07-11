using Marilog.OCR.Worker.Abstractions;
using PDFtoImage;
using SkiaSharp;
using System.Runtime.Versioning;
namespace Marilog.OCR.Worker.Infrastructure;

[SupportedOSPlatform("windows")]
[SupportedOSPlatform("linux")]
[SupportedOSPlatform("macos")]
public sealed class PdfThumbnailGenerator : IThumbnailGenerator
{
    public bool CanGenerate(string contentType) =>
        string.Equals(contentType, "application/pdf", StringComparison.OrdinalIgnoreCase);

    public Task<string?> GenerateAsync(string sourceFullPath, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        string thumbnailPath = Path.ChangeExtension(sourceFullPath, ".png");

        using var pdfStream = File.OpenRead(sourceFullPath);
        using var original = Conversion.ToImage(pdfStream, page: 0);

        const int maxSize = 300;

        float scale = Math.Min(
            (float)maxSize / original.Width,
            (float)maxSize / original.Height);

        scale = Math.Min(scale, 1f); // لا نقوم بتكبير الصور الصغيرة

        int width = (int)(original.Width * scale);
        int height = (int)(original.Height * scale);

        using var thumbnail = original.Resize(
            new SKImageInfo(width, height),
            SKSamplingOptions.Default);

        using var image = SKImage.FromBitmap(thumbnail);
        using var data = image.Encode(SKEncodedImageFormat.Png, 80);

        using var output = File.Create(thumbnailPath);
        data.SaveTo(output);

        return Task.FromResult<string?>(thumbnailPath);
    }
}