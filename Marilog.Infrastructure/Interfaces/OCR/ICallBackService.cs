namespace Marilog.Infrastructure.Interfaces.OCR
{
    public interface ICallBackService
    {
        Task NotifyOcrCompletedAsync(Guid documentId, string extractedContent, string? thumbnailPath, CancellationToken ct = default);
    }
}
