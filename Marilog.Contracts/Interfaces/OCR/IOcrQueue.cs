using Marilog.Contracts.DTOs.OCR;

namespace Marilog.Contracts.Interfaces.OCR
{
    public interface IOcrQueue
    {
        ValueTask EnqueueAsync(OcrRequest request, CancellationToken ct = default);
    }
}
