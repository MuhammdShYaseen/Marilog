using Marilog.OCR.Worker.Abstractions;
using Marilog.OCR.Worker.Domain;
using UglyToad.PdfPig;

namespace Marilog.OCR.Worker.Infrastructure
{
    public class PageAnalyzer : IPageAnalyzer
    {
        public List<PageAnalysisResult> Analyze(string pdfPath)
        {
            var results = new List<PageAnalysisResult>();

            using var doc = PdfDocument.Open(pdfPath);

            foreach (var page in doc.GetPages())
            {
                // ── هل يوجد نص حقيقي في الصفحة؟ ──
                var textLength = page.Text.Trim().Length;
                bool hasRealText = textLength > 10; // أكثر من 10 حرف = نص حقيقي

                // ── هل يوجد صور في الصفحة؟ ──
                bool hasImages = page.GetImages().Any();

                // ── القرار ──
                var pageType = (hasRealText, hasImages) switch
                {
                    (true, false) => PageType.TextOnly,      // نص فقط → لا OCR
                    (false, true) => PageType.ImageOnly,     // صورة فقط → OCR
                    (true, true) => PageType.Mixed,         // مختلط → OCR للصور
                    (false, false) => PageType.Empty,         // فارغة → تجاهل
                };

                results.Add(new PageAnalysisResult(
                    PageNumber: page.Number,
                    PageType: pageType,
                    NeedsOcr: pageType != PageType.TextOnly,
                    TextLength: textLength,
                    ImageCount: page.GetImages().Count()
                ));
            }

            return results;
        }
    }
}
