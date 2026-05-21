using Marilog.OCR.Worker.Abstractions;
using Marilog.OCR.Worker.Domain;
using System.Threading.Channels;

namespace Marilog.OCR.Worker;

public sealed class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly ISearchablePdfService _pdfService;

    // ── Channel داخلي يستقبل الطلبات ──
    // لاحقاً: سيُملأ من Domain Event Handler
    private readonly Channel<OcrRequest> _channel =
        Channel.CreateUnbounded<OcrRequest>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

    public Worker(
        ILogger<Worker> logger,
        ISearchablePdfService pdfService)
    {
        _logger = logger;
        _pdfService = pdfService;
    }

    // ── الواجهة العامة التي سيستدعيها الهاندلير لاحقاً ──
    public ValueTask EnqueueAsync(OcrRequest request, CancellationToken ct = default)
        => _channel.Writer.WriteAsync(request, ct);

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("Marilog OCR Worker started | MaxParallel: {Max}",
            Environment.ProcessorCount - 1);

        await foreach (var request in _channel.Reader.ReadAllAsync(ct))
        {
            // لا ننتظر — نعالج في الخلفية حتى لا نوقف الـ channel
            _ = ProcessAsync(request, ct);
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
            if (!File.Exists(request.FilePath))
            {
                _logger.LogWarning(
                    "File not found: {Path} | DocumentId: {Id}",
                    request.FilePath, request.DocumentId);
                return;
            }

            var options = new OcrOptions
            {
                Languages = "eng+ara",
                RenderDpi = 300,
                MinConfidence = 35f,
                KeepOriginalBackup = true
            };

            // output = overwrite على نفس الملف
            var result = await _pdfService.ProcessAsync(
                inputPdfPath: request.FilePath,
                outputPdfPath: request.FilePath,
                options: options,
                ct: ct
            );

            _logger.LogInformation(
                "OCR completed | DocumentId: {Id} | Pages: {Pages} | Words: {Words} | Duration: {Duration:F1}s",
                request.DocumentId,
                result.TotalPages,
                result.Pages.Sum(p => p.Words.Count),
                result.Duration.TotalSeconds
            );

            // ── لاحقاً هنا: إرسال النتيجة لـ Marilog.Presentation ──
            // عبر HTTP أو Domain Event أو SignalR
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "OCR failed | DocumentId: {Id}", request.DocumentId);
        }
    }
}

// ── الطلب الذي سيرسله الهاندلير لاحقاً ──
