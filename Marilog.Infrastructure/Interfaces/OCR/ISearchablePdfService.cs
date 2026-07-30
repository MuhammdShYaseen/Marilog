using Marilog.Infrastructure.Models.OCR;

namespace Marilog.Infrastructure.Interfaces.OCR
{
    public interface ISearchablePdfService
    {
        Task<OcrDocumentResult> ProcessAsync(string inputPdfPath, string outputPdfPath,
                                             OcrOptions? options = null, IProgress<OcrProgress>? progress = null,
                                             CancellationToken ct = default);
    }
}
