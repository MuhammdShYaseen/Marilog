using Marilog.Infrastructure.Interfaces.OCR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Marilog.Infrastructure.Services.OCR
{
    public class PdfConversionService : IPdfConversionService
    {
        private readonly ILogger<PdfConversionService> _logger;
        public PdfConversionService(ILogger<PdfConversionService> logger)
        {
            _logger = logger;
        }
        public async Task<bool> EnsurePdfAsync(string filePath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path is required.", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found.", filePath);

            var extension = Path.GetExtension(filePath);

            // Already PDF
            if (extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                return true;

            try
            {
                var outputPdf = Path.ChangeExtension(filePath, ".pdf");

                await ConvertToPdfAsync(filePath, outputPdf, cancellationToken);

                if (!File.Exists(outputPdf))
                    return false;

                // Replace the original file with the generated PDF.
                File.Delete(filePath);
                File.Move(outputPdf, filePath);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed converting {File} to PDF.", filePath);

                throw new InvalidOperationException(
                    $"Failed to convert '{Path.GetFileName(filePath)}' to PDF.",
                    ex);
            }
        }

        private async Task ConvertToPdfAsync(string filePath, string outputPdf, CancellationToken cancellationToken)
        {
            var libreOfficePath = @"C:\Program Files\LibreOffice\program\soffice.exe";

            if (!File.Exists(libreOfficePath))
                throw new FileNotFoundException("LibreOffice executable not found.", libreOfficePath);

            var outputDirectory = Path.GetDirectoryName(outputPdf);

            if (string.IsNullOrWhiteSpace(outputDirectory))
                throw new InvalidOperationException("Output directory is invalid.");

            var startInfo = new ProcessStartInfo
            {
                FileName = libreOfficePath,
                Arguments =
                    $"--headless --convert-to pdf --outdir \"{outputDirectory}\" \"{filePath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process
            {
                StartInfo = startInfo
            };

            process.Start();

            var error = await process.StandardError.ReadToEndAsync(cancellationToken);
            var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);

            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    $"LibreOffice conversion failed: {error}");
            }

            if (!File.Exists(outputPdf))
            {
                throw new InvalidOperationException(
                    $"PDF conversion failed. LibreOffice output: {output}");
            }
        }
    }
}
