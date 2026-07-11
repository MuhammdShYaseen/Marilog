namespace Marilog.OCR.Worker.Services
{
    public interface ICallBackService
    {
        Task NotifyOcrCompletedAsync(Guid documentId, string extractedContent, string? thumbnailPath, CancellationToken ct = default);
    }
}
