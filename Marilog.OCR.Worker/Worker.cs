
using Marilog.OCR.Worker.Abstractions;
using Marilog.OCR.Worker.Domain;
using Marilog.OCR.Worker.Services;

namespace Marilog.OCR.Worker;

public sealed class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly OcrQueue _queue;
    private readonly ISearchablePdfService _pdfService;
    private readonly ICallBackService _callbackService;
    private readonly IPdfCompressionService _compressionService;
    private readonly IFallbackSearchablePdfService _fallBackpdfService;
    public Worker(
        ILogger<Worker> logger,
        OcrQueue queue,
        ISearchablePdfService pdfService,
        IPdfCompressionService compressionService,
        ICallBackService callBack,
        IFallbackSearchablePdfService fallbackSearchablePdf)
    {
        _logger = logger;
        _queue = queue;
        _pdfService = pdfService;
        _callbackService = callBack;
        _compressionService = compressionService;
        _fallBackpdfService = fallbackSearchablePdf;
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
            _logger.LogError(ex, "Primary OCR failed | DocumentId: {Id}", request.DocumentId);
            try
            {
                result = await _fallBackpdfService.ProcessAsync(inputPdfPath: request.FilePath, outputPdfPath: request.FilePath, options: ocrOptions, ct: ct);

                _logger.LogInformation("Fallback pipeline succeeded | DocumentId: {Id}", request.DocumentId);
            }
            catch (Exception fallbackEx)
            {
                _logger.LogError(fallbackEx, "Both primary and fallback OCR pipelines failed | DocumentId: {Id}", request.DocumentId);
                return;
            }
        }

        await CleanupAsync(request.FilePath, request.DocumentId);

        var extractedContent = string.Join(" ", result.Pages.SelectMany(p => p.Words.Select(w => w.Text)));

        await _callbackService.NotifyOcrCompletedAsync(request.DocumentId, extractedContent, ct);
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