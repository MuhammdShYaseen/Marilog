using Marilog.OCR.Worker.Abstractions;
using Marilog.OCR.Worker.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace Marilog.OCR.Worker.Infrastructure;

/// <summary>
/// Fallback pipeline يستخدم OCRmyPDF (subprocess) عند فشل الـ pipeline المخصص
/// (IText7PdfBuilder + TesseractOcrEngine + PageAnalyzer).
/// لا يُستخدم كخيار افتراضي — فقط عند catch من الـ primary service.
/// </summary>
public sealed class OcrMyPdfFallbackService : IFallbackSearchablePdfService
{
    private readonly ILogger<OcrMyPdfFallbackService> _logger;
    private readonly string _ocrMyPdfExecutable;

    public OcrMyPdfFallbackService(
        ILogger<OcrMyPdfFallbackService> logger)
    {
        _logger = logger;

        _ocrMyPdfExecutable = ResolveOcrMyPdfExecutable();
    }

    private static string ResolveOcrMyPdfExecutable()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return "ocrmypdf";
        }

        // Linux / macOS
        return "ocrmypdf";
    }

    public async Task<OcrDocumentResult> ProcessAsync(
        string inputPdfPath,
        string outputPdfPath,
        OcrOptions? options = null,
        IProgress<OcrProgress>? progress = null,
        CancellationToken ct = default)
    {
        options ??= new OcrOptions();
        var stopwatch = Stopwatch.StartNew();

        if (!File.Exists(inputPdfPath))
            throw new FileNotFoundException("Input PDF not found", inputPdfPath);

        var tempOutputPath = outputPdfPath + ".ocrmypdf.tmp";
        var sidecarPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.sidecar.txt");

        var psi = new ProcessStartInfo
        {
            FileName = _ocrMyPdfExecutable,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        // --skip-text: لا يلمس الصفحات النصية، تماماً زي منطق PageAnalyzer عندك
        // --rasterizer pypdfium: يتجنب الاعتماد على Ghostscript AGPL في خطوة الرندرة
        psi.ArgumentList.Add("--skip-text");
        psi.ArgumentList.Add("-l");
        psi.ArgumentList.Add(options.Languages); // "eng+ara" — نفس صيغة Tesseract الحالية
        psi.ArgumentList.Add("--rasterizer");
        psi.ArgumentList.Add("pypdfium");
        psi.ArgumentList.Add("--sidecar");
        psi.ArgumentList.Add(sidecarPath);
        psi.ArgumentList.Add("--jobs");
        psi.ArgumentList.Add("2");
        psi.ArgumentList.Add(inputPdfPath);
        psi.ArgumentList.Add(tempOutputPath);

        _logger.LogInformation(
            "OCRmyPDF fallback started | File: {File}",
            Path.GetFileName(inputPdfPath));

        Process? process = null;

        try
        {
            process = Process.Start(psi)
                ?? throw new InvalidOperationException("Failed to start OCRmyPDF process");

            var stdoutTask = process.StandardOutput.ReadToEndAsync(ct);
            var stderrTask = process.StandardError.ReadToEndAsync(ct);

            try
            {
                await process.WaitForExitAsync(ct);
            }
            catch (OperationCanceledException)
            {
                if (!process.HasExited)
                    process.Kill(entireProcessTree: true);
                throw;
            }

            var stderr = await stderrTask;
            _ = await stdoutTask;

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    $"OCRmyPDF exited with code {process.ExitCode}: {stderr}");
            }

            if (!File.Exists(tempOutputPath))
                throw new InvalidOperationException("OCRmyPDF did not produce an output file");

            File.Move(tempOutputPath, outputPdfPath, overwrite: true);

            var pages = BuildPageResults(outputPdfPath, sidecarPath);

            stopwatch.Stop();

            _logger.LogInformation(
                "OCRmyPDF fallback completed | Pages: {Pages} | Duration: {Duration:F1}s",
                pages.Count, stopwatch.Elapsed.TotalSeconds);

            return new OcrDocumentResult(
                inputPdfPath, outputPdfPath, pages.Count, pages.Count, pages, stopwatch.Elapsed);
        }
        finally
        {
            process?.Dispose();
            if (File.Exists(tempOutputPath)) File.Delete(tempOutputPath);
            if (File.Exists(sidecarPath)) File.Delete(sidecarPath);
        }
    }

    /// <summary>
    /// OCRmyPDF ما يعطينا bounding boxes لكل كلمة زي محرك Tesseract المباشر عندك،
    /// فبنبني "كلمة" واحدة صناعية لكل صفحة تحتوي على كامل نص الصفحة من الـ sidecar file
    /// (مفصولة بـ form-feed \f). هذا كافٍ للـ callback واستخراج النص،
    /// لكن مش دقيق لإعادة بناء طبقة نص بإحداثيات — وهذا غير مطلوب هنا أصلاً
    /// لأن OCRmyPDF بنى الطبقة النهائية بنفسه.
    /// </summary>
    private List<OcrPageResult> BuildPageResults(string outputPdfPath, string sidecarPath)
    {
        var pagesText = File.Exists(sidecarPath)
            ? File.ReadAllText(sidecarPath).Split('\f')
            : [];

        using var doc = PdfDocument.Open(outputPdfPath);
        var results = new List<OcrPageResult>();
        int i = 0;

        foreach (var page in doc.GetPages())
        {
            var text = i < pagesText.Length ? pagesText[i].Trim() : string.Empty;

            var words = string.IsNullOrWhiteSpace(text)
                ? new List<OcrWord>()
                : new List<OcrWord>
                {
            new OcrWord(
                Text: text,
                Confidence: 100f,
                BoundingBox: new OcrBoundingBox(
                    X1: 0,
                    Y1: 0,
                    X2: (int)page.Width,
                    Y2: (int)page.Height))
                };

            results.Add(new OcrPageResult(
                PageNumber: page.Number,
                Words: words,
                PageWidthPx: (int)page.Width,
                PageHeightPx: (int)page.Height));

            i++;
        }

        return results;
    }
}