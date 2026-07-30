using Marilog.Application.Interfaces.OCR;
using Marilog.Contracts.DTOs.OCR;
using Marilog.Infrastructure.Queues.OCR;
using Microsoft.Extensions.Logging;


namespace Marilog.Infrastructure.Services.OCR
{
    public sealed class OcrStarter : IOcrStarter
    {
        private readonly string _basePath;
        private readonly ILogger _logger;
        private readonly OcrQueue _ocrQueue;
        public OcrStarter(string basePath, ILogger logger, OcrQueue ocrQueue)
        {
            _basePath = basePath;
            _logger = logger;
            _ocrQueue = ocrQueue;
        }

        public async Task<bool> StartOcr(OcrRequest request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.FilePath))
            {
                _logger.LogWarning(
                    "OCR request rejected: FilePath is missing | DocumentId: {DocumentId}",
                    request.DocumentId);

                return false;
            }

            request.FilePath = Path.Combine(_basePath, request.FilePath);

            if (!File.Exists(request.FilePath))
            {
                _logger.LogWarning(
                    "OCR request rejected: File not found | DocumentId: {DocumentId} | File: {File}",
                    request.DocumentId,
                    request.FilePath);

                return false;
            }

            _logger.LogInformation(
                "OCR request received | DocumentId: {DocumentId} | File: {File}",
                request.DocumentId,
                Path.GetFileName(request.FilePath));

            await _ocrQueue.EnqueueAsync(request, ct);

            return true;
        }
    }
}
