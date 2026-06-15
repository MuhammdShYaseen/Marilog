using Marilog.OCR.Worker.Abstractions;
using Marilog.OCR.Worker.Domain;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using TesseractOCR;
using TesseractOCR.Enums;

namespace Marilog.OCR.Worker.Infrastructure;

public sealed class SearchablePdfService : ISearchablePdfService
{
    private readonly ILogger<SearchablePdfService> _logger;
    private readonly IPdfRenderer _renderer;
    private readonly IOcrEngineFactory _ocrFactory;
    private readonly IText7PdfBuilder _builder;  // ✅ اسم جديد
    private readonly IPageAnalyzer _pageAnalyzer;
    public SearchablePdfService(ILogger<SearchablePdfService> logger, IPdfRenderer renderer, 
                                IOcrEngineFactory ocrFactory, IText7PdfBuilder builder, IPageAnalyzer pageAnalyzer)
    {
        _logger = logger;
        _renderer = renderer;
        _ocrFactory = ocrFactory;
        _builder = builder;
        _pageAnalyzer = pageAnalyzer;
    }

    public async Task<OcrDocumentResult> ProcessAsync(string inputPdfPath, string outputPdfPath, OcrOptions? options = null,
                                                  IProgress<OcrProgress>? progress = null, CancellationToken ct = default)
    {
        options ??= new OcrOptions();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        if (!File.Exists(inputPdfPath))
            throw new FileNotFoundException("Input PDF not found", inputPdfPath);

        if (options.KeepOriginalBackup)
        {
            var backupPath = inputPdfPath + ".original.bak";
            if (!File.Exists(backupPath))
            {
                File.Copy(inputPdfPath, backupPath);
                _logger.LogInformation("Backup created: {Backup}", backupPath);
            }
        }

        var pageAnalysis = _pageAnalyzer.Analyze(inputPdfPath);
        var pagesNeedingOcr = pageAnalysis.Where(p => p.NeedsOcr).ToList();
        var pagesTextOnly = pageAnalysis.Where(p => !p.NeedsOcr).ToList();
        int totalPages = pageAnalysis.Count;

        _logger.LogInformation(
            "Processing: {File} ({Pages} pages) | Lang: {Lang} | DPI: {Dpi} | NeedsOcr: {Ocr} | TextOnly: {Text}",
            Path.GetFileName(inputPdfPath), totalPages,
            options.Languages, options.RenderDpi,
            pagesNeedingOcr.Count, pagesTextOnly.Count
        );

        if (pagesNeedingOcr.Count == 0)
        {
            _logger.LogInformation("All pages are text-based — OCR skipped");
            stopwatch.Stop();
            return new OcrDocumentResult(inputPdfPath, inputPdfPath, totalPages, 0, [], stopwatch.Elapsed);
        }

        progress?.Report(new OcrProgress(0, pagesNeedingOcr.Count, "Starting..."));

        // ── النتائج بترتيب ثابت ──
        var results = new (OcrPageResult Page, byte[] Image)[pagesNeedingOcr.Count];
        int processed = 0;

        int batchSize = options.BatchSize > 0 ? options.BatchSize : 4;
        int maxDegreeOfParallelism = options.MaxDegreeOfParallelism > 0 ? options.MaxDegreeOfParallelism : 2;

        var batches = pagesNeedingOcr
            .Select((analysis, i) => (analysis, i))
            .Chunk(batchSize);

        foreach (var batch in batches)
        {
            ct.ThrowIfCancellationRequested();

            await Parallel.ForEachAsync(
                batch,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = maxDegreeOfParallelism,
                    CancellationToken = ct
                },
                async (item, innerCt) =>
                {
                    var (analysis, index) = item;
                    int pageIndex = analysis.PageNumber - 1;

                    // ── Render: I/O bound ──
                    var imageBytes = await _renderer.RenderPageToImageAsync(
                        inputPdfPath, pageIndex, options.RenderDpi, innerCt
                    );

                    var (widthPx, heightPx) = GetImageDimensions(imageBytes);

                    // ── OCR: engine منفصل لكل thread لأن Tesseract ليس thread-safe ──
                    using var engine = _ocrFactory.Create(options);
                    var words = await engine.RecognizeAsync(imageBytes, innerCt);

                    results[index] = (
                        new OcrPageResult(analysis.PageNumber, words, widthPx, heightPx),
                        imageBytes
                    );

                    int current = Interlocked.Increment(ref processed);

                    _logger.LogDebug(
                        "Page {Page}/{Total}: {Words} words | Type: {Type}",
                        analysis.PageNumber, totalPages, words.Count, analysis.PageType
                    );

                    progress?.Report(new OcrProgress(
                        current,
                        pagesNeedingOcr.Count,
                        $"Processing page {analysis.PageNumber}/{totalPages} ({analysis.PageType})"
                    ));
                }
            );
        }

        progress?.Report(new OcrProgress(pagesNeedingOcr.Count, pagesNeedingOcr.Count, "Building searchable PDF..."));

        // ── pageData مرتبة بنفس ترتيب pagesNeedingOcr ──
        var pageData = results.Select(r => (r.Page, r.Image)).ToList();
        var ocrPages = results.Select(r => r.Page).ToList();

        await _builder.BuildWithImagesAsync(outputPdfPath, pageData, options.RenderDpi, ct);

        stopwatch.Stop();

        _logger.LogInformation(
            "✓ Done in {Duration:F1}s | {Total} pages | {Ocr} OCR'd | {Skipped} skipped | {Words} words | Output: {Output}",
            stopwatch.Elapsed.TotalSeconds, totalPages, processed,
            pagesTextOnly.Count, ocrPages.Sum(p => p.Words.Count),
            Path.GetFileName(outputPdfPath)
        );

        progress?.Report(new OcrProgress(pagesNeedingOcr.Count, pagesNeedingOcr.Count, "Complete"));

        return new OcrDocumentResult(inputPdfPath, outputPdfPath, totalPages, processed, ocrPages, stopwatch.Elapsed);
    }

    // ✅ SkiaSharp بدلاً من SixLabors.ImageSharp
    private static (int Width, int Height) GetImageDimensions(byte[] imageBytes)
    {
        using var bitmap = SKBitmap.Decode(imageBytes);
        return (bitmap.Width, bitmap.Height);
    }

}

// ═══════════════════════════════════════════════════════
//  IOcrEngineFactory + TesseractOcrEngineFactory
// ═══════════════════════════════════════════════════════

public interface IOcrEngineFactory
{
    IOcrEngine Create(OcrOptions options);
}

public sealed class TesseractOcrEngineFactory : IOcrEngineFactory
{
    private readonly ILogger<TesseractOcrEngine> _logger;
    private readonly string _defaultTessDataPath;

    public TesseractOcrEngineFactory(
        ILogger<TesseractOcrEngine> logger,
        string defaultTessDataPath)
    {
        _logger = logger;
        _defaultTessDataPath = defaultTessDataPath;
    }

    public IOcrEngine Create(OcrOptions options)
    {
        var tessDataPath = options.TessDataPath ?? _defaultTessDataPath;

        return new TesseractOcrEngine(
            _logger,
            tessDataPath,
            options.Languages,
            options.MinConfidence
        );
    }
}