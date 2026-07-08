
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
    public Worker(
        ILogger<Worker> logger,
        OcrQueue queue,
        ISearchablePdfService pdfService,
        IPdfCompressionService compressionService,
        ICallBackService callBack)
    {
        _logger = logger;
        _queue = queue;
        _pdfService = pdfService;
        _callbackService = callBack;
        _compressionService = compressionService;
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
        _logger.LogInformation(
            "OCR started | DocumentId: {Id} | File: {File}",
            request.DocumentId,
            Path.GetFileName(request.FilePath)
        );

        try
        {
            var result = await _pdfService.ProcessAsync(
                inputPdfPath: request.FilePath,
                outputPdfPath: request.FilePath,
                options: new OcrOptions
                {
                    Languages = "eng+ara",
                    RenderDpi = 300,
                    MinConfidence = 35f,
                    KeepOriginalBackup = true
                },
                ct: ct
            );

            _logger.LogInformation(
                "OCR completed | DocumentId: {Id} | Pages: {Pages} | Words: {Words} | Duration: {Duration:F1}s",
                request.DocumentId,
                result.TotalPages,
                result.Pages.Sum(p => p.Words.Count),
                result.Duration.TotalSeconds
            );

            await CleanupAsync(request.FilePath, request.DocumentId);

            var extractedContent = string.Join(" ",
            result.Pages.SelectMany(p => p.Words.Select(w => w.Text)));

            await _callbackService.NotifyOcrCompletedAsync(
                request.DocumentId, extractedContent, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "OCR failed | DocumentId: {Id}", request.DocumentId);
        }
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
            _logger.LogWarning(ex,
                "Compression step failed, continuing | DocumentId: {Id}", documentId);
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