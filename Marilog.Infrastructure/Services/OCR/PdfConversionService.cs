using Marilog.Infrastructure.Interfaces.OCR;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkiaSharp;

namespace Marilog.Infrastructure.Services.OCR
{
    /// <summary>
    /// Gives the OCR job a PDF: PDFs pass through as they are; images get a one-page PDF copy next to them
    /// ("{file}.pdf", e.g. guid.jpg → guid.jpg.pdf) sized at 300 DPI so OCRmyPDF reads them well.
    /// The original is never touched. Any other type returns null.
    /// </summary>
    public class PdfConversionService : IPdfConversionService
    {
        private const float Dpi = 300f;

        public Task<string?> EnsurePdfAsync(string filePath, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found.", filePath);

            var ext = Path.GetExtension(filePath).ToLowerInvariant();

            if (ext == ".pdf")
                return Task.FromResult<string?>(filePath);

            if (ext is not (".jpg" or ".jpeg" or ".png" or ".bmp" or ".gif" or ".webp"))
                return Task.FromResult<string?>(null);

            using var codec = SKCodec.Create(filePath)
                ?? throw new InvalidOperationException($"'{Path.GetFileName(filePath)}' is not a readable image.");

            // Page size = image size at 300 DPI, so OCRmyPDF treats it like a 300 DPI scan.
            var width = codec.Info.Width * 72f / Dpi;
            var height = codec.Info.Height * 72f / Dpi;
            var pdfCopy = filePath + ".pdf";

            QuestPDF.Settings.License = LicenseType.Community;
            Document.Create(doc => doc.Page(page =>
            {
                page.Size(new PageSize(width, height));
                page.Margin(0);
                page.Content().Image(filePath).FitArea();
            })).GeneratePdf(pdfCopy);

            return Task.FromResult<string?>(pdfCopy);
        }
    }
}