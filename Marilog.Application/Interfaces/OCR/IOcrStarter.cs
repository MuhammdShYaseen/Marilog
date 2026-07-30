using Marilog.Contracts.DTOs.OCR;

namespace Marilog.Application.Interfaces.OCR
{
    public interface IOcrStarter
    {
        Task<bool> StartOcr(OcrRequest request, CancellationToken ct);
    }
}
