using Marilog.OCR.Worker.Abstractions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Marilog.OCR.Worker.Infrastructure;

public sealed class GhostscriptPdfCompressionService : IPdfCompressionService
{
    private readonly ILogger<GhostscriptPdfCompressionService> _logger;
    private readonly string _gsExecutable;

    public GhostscriptPdfCompressionService(ILogger<GhostscriptPdfCompressionService> logger)
    {
        _logger = logger;
        _gsExecutable = ResolveGsExecutable();
    }

    private static string ResolveGsExecutable()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // نفضل console (c) عشان ما تفتح نافذة، ونفضل 64-bit
            return Environment.Is64BitOperatingSystem ? "gswin64c" : "gswin32c";
        }

        // Linux/macOS
        return "gs";
    }

    public async Task<bool> CompressAsync(string pdfPath, CancellationToken ct = default)
    {
        if (!File.Exists(pdfPath)) return false;

        var tempPath = pdfPath + ".compressed.tmp";
        var originalSize = new FileInfo(pdfPath).Length;

        var psi = new ProcessStartInfo
        {
            FileName = _gsExecutable,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        // /ebook = ~150 DPI للصور، توازن ممتاز جودة/حجم (هذا نفس الـ preset اللي PDF24 يستخدمه بالوضع الافتراضي)
        // لو الجودة مهمة أكثر من الحجم، استخدم /printer بدل /ebook (300 DPI)
        psi.ArgumentList.Add("-sDEVICE=pdfwrite");
        psi.ArgumentList.Add("-dCompatibilityLevel=1.5");
        psi.ArgumentList.Add("-dPDFSETTINGS=/ebook");
        psi.ArgumentList.Add("-dNOPAUSE");
        psi.ArgumentList.Add("-dQUIET");
        psi.ArgumentList.Add("-dBATCH");
        psi.ArgumentList.Add("-dSAFER");
        psi.ArgumentList.Add($"-sOutputFile={tempPath}");
        psi.ArgumentList.Add(pdfPath);

        try
        {
            using var process = Process.Start(psi)
                ?? throw new InvalidOperationException("Failed to start Ghostscript process");

            var stderrTask = process.StandardError.ReadToEndAsync(ct);
            await process.WaitForExitAsync(ct);
            var stderr = await stderrTask;

            if (process.ExitCode != 0 || !File.Exists(tempPath))
            {
                _logger.LogWarning(
                    "Ghostscript compression failed (exit {Code}) | File: {File} | {Error}",
                    process.ExitCode, Path.GetFileName(pdfPath), stderr);
                if (File.Exists(tempPath)) File.Delete(tempPath);
                return false;
            }

            var compressedSize = new FileInfo(tempPath).Length;

            if (compressedSize >= originalSize)
            {
                _logger.LogInformation(
                    "Compression skipped — no size benefit ({Before:N0} KB → {After:N0} KB) | File: {File}",
                    originalSize / 1024, compressedSize / 1024, Path.GetFileName(pdfPath));
                File.Delete(tempPath);
                return false;
            }

            File.Move(tempPath, pdfPath, overwrite: true);

            _logger.LogInformation(
                "PDF compressed: {Before:N0} KB → {After:N0} KB ({Pct:P0} reduction) | File: {File}",
                originalSize / 1024, compressedSize / 1024,
                1 - (double)compressedSize / originalSize, Path.GetFileName(pdfPath));

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Compression error | File: {File}", Path.GetFileName(pdfPath));
            if (File.Exists(tempPath)) File.Delete(tempPath);
            return false;
        }
    }
}