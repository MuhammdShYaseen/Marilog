
namespace Marilog.Infrastructure.OCR.Interfaces
{
    public interface ICallBackService
    {
        Task NotifyOcrCompletedAsync(Guid documentId, string extractedContent, string? thumbnailPath, CancellationToken ct = default);
    }
}
