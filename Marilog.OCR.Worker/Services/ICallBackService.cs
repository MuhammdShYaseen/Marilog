namespace Marilog.OCR.Worker.Services
{
    public interface ICallBackService
    {
        Task NotifyOcrCompletedAsync(int documentId, string extractedContent, CancellationToken ct = default);
    }
}
