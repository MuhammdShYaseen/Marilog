
using Marilog.Contracts.DTOs.OCR;
using Marilog.Infrastructure.Interfaces.OCR;
using Marilog.Infrastructure.Models.OCR;
using Marilog.Infrastructure.Queues.OCR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Marilog.Infrastructure.BackgroundServices
{
    public sealed class OcrWorker : BackgroundService
    {
        private readonly ILogger<OcrWorker> _logger;
        private readonly OcrQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;
        public OcrWorker(
            ILogger<OcrWorker> logger,
            OcrQueue queue,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _queue = queue;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            _logger.LogInformation("Marilog OCR Worker ready");

            await foreach (var request in _queue.Reader.ReadAllAsync(ct))
            {
                // لا ننتظر — نعالج في الخلفية
                _ = ProcessAsync(request, CancellationToken.None);
            }
        }

        private async Task ProcessAsync(OcrRequest request, CancellationToken ct)
        {
            _logger.LogInformation("OCR started | DocumentId: {Id} | File: {File}", request.DocumentId, Path.GetFileName(request.FilePath));
            using var scope = _scopeFactory.CreateScope();
            var _pdfService = scope.ServiceProvider.GetRequiredService<ISearchablePdfService>();
            var _callbackService = scope.ServiceProvider.GetRequiredService<ICallBackService>();
            var _compressionService = scope.ServiceProvider.GetRequiredService<IPdfCompressionService>();
            var _pdfDirectText = scope.ServiceProvider.GetRequiredService<IPdfDirectTextExtractor>();
            var _pdfThumbnail = scope.ServiceProvider.GetRequiredService<IThumbnailGenerator>();

            OcrDocumentResult result;

            var ocrOptions = new OcrOptions
            {
                Languages = "eng+ara",
                RenderDpi = 300,
                MinConfidence = 35f,
                KeepOriginalBackup = true
            };



            try
            {
                result = await _pdfService.ProcessAsync(inputPdfPath: request.FilePath, outputPdfPath: request.FilePath, ocrOptions, ct: ct);
            }
            catch (Exception ex)
            {
                
                _logger.LogError(ex, "OCR pipelines failed | DocumentId: {Id}", request.DocumentId);
                return;
            }

            var extractedContent = _pdfDirectText.ExtractText(request.FilePath, ct);
            var thumbnailPath = await _pdfThumbnail.GenerateAsync(request.FilePath);
            if (!string.IsNullOrWhiteSpace(extractedContent))
            {
                await _callbackService.NotifyOcrCompletedAsync(request.DocumentId, extractedContent, thumbnailPath, ct);
            }
            else
            {
                _logger.LogError("extracted content failed | DocumentId: {Id}", request.DocumentId);
            }

            await CleanupAsync(request.FilePath, request.DocumentId, _compressionService);
        }

        private async Task CleanupAsync(string filePath, Guid documentId, IPdfCompressionService _compressionService)
        {
            // كمبريس — best-effort، ما لازم يفشل الـ job لو صار خطأ هنا
            try
            {
                await _compressionService.CompressAsync(filePath, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Compression step failed, continuing | DocumentId: {Id}", documentId);
            }

            // حذف نسخة الـ backup
            var backupPath = filePath + ".original.bak";
            try
            {
                if (File.Exists(backupPath))
                {
                    File.Delete(backupPath);
                    _logger.LogInformation(
                        "Backup deleted | DocumentId: {Id} | File: {File}",
                        documentId, Path.GetFileName(backupPath));
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to delete backup | DocumentId: {Id} | File: {File}",
                    documentId, Path.GetFileName(backupPath));
            }
        }
    }
}
