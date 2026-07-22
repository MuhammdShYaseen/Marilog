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

        const int maxSize = 300;

        using var pdfStream = File.OpenRead(sourceFullPath);

        // رندر الصفحة بدقة أعلى (300 DPI) بدل الاعتماد على الافتراضي المنخفض
        // ده بيدي صورة أصلية أوضح، والتصغير لاحقاً بيحافظ على تفاصيل أكتر (supersampling)
        using var original = Conversion.ToImage(
            pdfStream,
            page: 0,
            options: new RenderOptions(Dpi: 300));

        float scale = Math.Min(
            (float)maxSize / original.Width,
            (float)maxSize / original.Height);

        scale = Math.Min(scale, 1f); // لا نقوم بتكبير الصور الصغيرة

        int width = (int)(original.Width * scale);
        int height = (int)(original.Height * scale);

        using var thumbnail = original.Resize(
            new SKImageInfo(width, height),
            new SKSamplingOptions(SKCubicResampler.Mitchell)); // فلتر تصغير عالي الجودة بدل Default

        using var image = SKImage.FromBitmap(thumbnail);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100); // PNG أساساً بدون فقد، رفعناها لأقصى قيمة احتياطاً

        using var output = File.Create(thumbnailPath);
        data.SaveTo(output);

        return Task.FromResult<string?>(thumbnailPath);
    }
}