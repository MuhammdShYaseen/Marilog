
using Marilog.Contracts.DTOs.OCR;
using Marilog.Infrastructure.OCR.Interfaces;
using Marilog.Infrastructure.OCR.Models;
using Marilog.Infrastructure.OCR.Queues;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Marilog.Infrastructure.BackgroundServices
{
    public sealed class OcrWorker : BackgroundService
    {
        private readonly ILogger<OcrWorker> _logger;
        private readonly OcrQueue _queue;
        private readonly ISearchablePdfService _pdfService;
        private readonly ICallBackService _callbackService;
        private readonly IPdfCompressionService _compressionService;
        private readonly IPdfDirectTextExtractor _pdfDirectText;
        private readonly IThumbnailGenerator _pdfThumbnail;
        public OcrWorker(
            ILogger<OcrWorker> logger,
            OcrQueue queue,
            ISearchablePdfService pdfService,
            IPdfCompressionService compressionService,
            ICallBackService callBack,
            IPdfDirectTextExtractor pdfDirectText,
            IThumbnailGenerator thumbnailGenerator)
        {
            _logger = logger;
            _queue = queue;
            _pdfService = pdfService;
            _callbackService = callBack;
            _compressionService = compressionService;
            _pdfDirectText = pdfDirectText;
            _pdfThumbnail = thumbnailGenerator;
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

            await CleanupAsync(request.FilePath, request.DocumentId);
        }

        private async Task CleanupAsync(string filePath, Guid documentId)
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
