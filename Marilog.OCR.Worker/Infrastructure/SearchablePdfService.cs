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

        // ── تحليل الملف أولاً ──
        var pageAnalysis = _pageAnalyzer.Analyze(inputPdfPath);

        var pagesNeedingOcr = pageAnalysis.Where(p => p.NeedsOcr).ToList();
        var pagesTextOnly = pageAnalysis.Where(p => !p.NeedsOcr).ToList();

        int totalPages = pageAnalysis.Count;

        _logger.LogInformation(
            "Processing: {File} ({Pages} pages) | Lang: {Lang} | DPI: {Dpi} | NeedsOcr: {Ocr} | TextOnly: {Text}",
            Path.GetFileName(inputPdfPath),
            totalPages,
            options.Languages,
            options.RenderDpi,
            pagesNeedingOcr.Count,
            pagesTextOnly.Count
        );

        // ── إذا كل الصفحات نصية → لا حاجة لـ OCR ──
        if (pagesNeedingOcr.Count == 0)
        {
            _logger.LogInformation("All pages are text-based — OCR skipped");

            stopwatch.Stop();

            return new OcrDocumentResult(
                InputPath: inputPdfPath,
                OutputPath: inputPdfPath,
                TotalPages: totalPages,
                ProcessedPages: 0,
                Pages: [],
                Duration: stopwatch.Elapsed
            );
        }

        progress?.Report(new OcrProgress(0, pagesNeedingOcr.Count, "Starting..."));

        using var ocrEngine = _ocrFactory.Create(options);

        var ocrPages = new List<OcrPageResult>(pagesNeedingOcr.Count);
        var pageData = new List<(OcrPageResult, byte[])>(pagesNeedingOcr.Count);
        int processed = 0;

        // ── معالجة الصفحات التي تحتاج OCR فقط ──
        foreach (var analysis in pagesNeedingOcr)
        {
            ct.ThrowIfCancellationRequested();

            // pageIndex = pageNumber - 1
            int pageIndex = analysis.PageNumber - 1;

            progress?.Report(new OcrProgress(
                processed,
                pagesNeedingOcr.Count,
                $"Processing page {analysis.PageNumber}/{totalPages} ({analysis.PageType})"
            ));

            // ── خطوة 1: PDF → PNG bytes ──
            var imageBytes = await _renderer.RenderPageToImageAsync(
                inputPdfPath, pageIndex, options.RenderDpi, ct
            );

            // ── خطوة 2: أبعاد الصورة ──
            var (widthPx, heightPx) = GetImageDimensions(imageBytes);

            // ── خطوة 3: OCR ──
            var words = await ocrEngine.RecognizeAsync(imageBytes, ct);

            var ocrPage = new OcrPageResult(
                PageNumber: analysis.PageNumber,
                Words: words,
                PageWidthPx: widthPx,
                PageHeightPx: heightPx
            );

            ocrPages.Add(ocrPage);
            pageData.Add((ocrPage, imageBytes));
            processed++;

            _logger.LogDebug(
                "Page {Page}/{Total}: {Words} words | Type: {Type}",
                analysis.PageNumber, totalPages, words.Count, analysis.PageType
            );
        }

        progress?.Report(new OcrProgress(pagesNeedingOcr.Count, pagesNeedingOcr.Count, "Building searchable PDF..."));

        await _builder.BuildWithImagesAsync(
            outputPdfPath,
            pageData,
            options.RenderDpi,
            ct
        );

        stopwatch.Stop();

        var result = new OcrDocumentResult(
            InputPath: inputPdfPath,
            OutputPath: outputPdfPath,
            TotalPages: totalPages,
            ProcessedPages: processed,
            Pages: ocrPages,
            Duration: stopwatch.Elapsed
        );

        _logger.LogInformation(
            "✓ Done in {Duration:F1}s | {Total} pages | {Ocr} OCR'd | {Skipped} skipped | {Words} words | Output: {Output}",
            stopwatch.Elapsed.TotalSeconds,
            totalPages,
            processed,
            pagesTextOnly.Count,
            ocrPages.Sum(p => p.Words.Count),
            Path.GetFileName(outputPdfPath)
        );

        progress?.Report(new OcrProgress(pagesNeedingOcr.Count, pagesNeedingOcr.Count, "Complete"));

        return result;
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