using Marilog.Application.Interfaces.DocumentExtraction;
using Marilog.Application.Models.DocumentExtraction;
using UglyToad.PdfPig;

namespace Marilog.Infrastructure.Services.DocumentExtraction
{
    internal sealed class PdfPigWordReader : IPdfWordReader
    {
        public IReadOnlyList<PdfPageWords> Read(byte[] pdf)
        {
            using var document = PdfDocument.Open(pdf);

            return document.GetPages()
                .Select(page => new PdfPageWords(
                    page.Number,
                    page.Width,
                    page.Height,
                    page.GetWords()
                        .Where(w => !string.IsNullOrWhiteSpace(w.Text))
                        .Select(w => new PdfWord(
                            w.Text,
                            w.BoundingBox.Left,
                            w.BoundingBox.Right,
                            w.BoundingBox.Top,
                            w.BoundingBox.Bottom))
                        .ToList()))
                .ToList();
        }
    }
}