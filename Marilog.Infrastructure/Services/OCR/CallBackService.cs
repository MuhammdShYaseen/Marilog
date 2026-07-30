using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Infrastructure.Interfaces.OCR;
using Microsoft.Extensions.Logging;

namespace Marilog.Infrastructure.Services.OCR
{
    public sealed class CallBackService : ICallBackService
    {
        private readonly ILogger<CallBackService> _logger;
        private readonly IStoredFileService _storedFileService;
        public CallBackService(ILogger<CallBackService> logger, IStoredFileService storedFileService)
        {
            _logger = logger;
            _storedFileService = storedFileService;
        }

        public async Task NotifyOcrCompletedAsync(Guid documentId, string extractedContent, string? thumbnailPath, CancellationToken ct = default)
        {
            try
            {
                await _storedFileService.UpdateContentFromOCRAsync(documentId, extractedContent, thumbnailPath, ct);
                _logger.LogInformation("Callback succeeded | DocumentId: {Id}", documentId);
            }
            catch (Exception ex) 
            {
                var error = ex;
                _logger.LogError("Callback failed | DocumentId: {Id} | Status: {Status} | Error: {Error}",
                                        documentId, "error" , error);
            }
            
        }
    }
}