
using Marilog.Infrastructure.Models.OCR;

namespace Marilog.Infrastructure.OCR.Interfaces
{
    public interface ISearchablePdfService
    {
        Task<OcrDocumentResult> ProcessAsync(string inputPdfPath, string outputPdfPath,
                                             OcrOptions? options = null, IProgress<OcrProgress>? progress = null,
                                             CancellationToken ct = default);
    }
}
