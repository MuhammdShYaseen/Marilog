using Marilog.OCR.Worker.Abstractions;
using System.Text;
using UglyToad.PdfPig;

namespace Marilog.OCR.Worker.Infrastructure
{
    public class PdfDirectTextExtractor : IPdfDirectTextExtractor
    {
        public string ExtractText(string filePath, CancellationToken cancellationToken = default)
        {
            var builder = new StringBuilder();

            using var document = PdfDocument.Open(filePath);

            foreach (var page in document.GetPages())
            {
                cancellationToken.ThrowIfCancellationRequested();
                builder.AppendLine(page.Text);
            }

            return builder.ToString();
        }
    }
}
