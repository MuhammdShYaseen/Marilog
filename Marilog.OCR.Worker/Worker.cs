
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
    public Worker(
        ILogger<Worker> logger,
        OcrQueue queue,
        ISearchablePdfService pdfService,
        ICallBackService callBack)
    {
        _logger = logger;
        _queue = queue;
        _pdfService = pdfService;
        _callbackService = callBack;
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

            // ── لاحقاً: إخبار Marilog.Presentation بالنتيجة ──
            var extractedContent = string.Join("\n",
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
}