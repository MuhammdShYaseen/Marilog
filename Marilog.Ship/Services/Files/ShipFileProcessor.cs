using Marilog.Application.Interfaces.OCR;
using Marilog.Contracts.DTOs.OCR;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Infrastructure.Interfaces.OCR;
using Marilog.Infrastructure.Services.OCR;
using Marilog.Ship.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Marilog.Ship.Services.Files
{
    /// <summary>
    /// The ship's IOcrStarter: the same steps as Marilog's OcrWorker, without the OCR step.
    ///   • Word/Excel/PowerPoint/txt → text + "{file}.html" for the preview panel.
    ///   • PDF → its text layer + thumbnail.
    ///   • Image → "{file}.pdf" copy → thumbnail (no text without OCR).
    /// The extractors are Marilog.Infrastructure's own classes, linked into this project (see the csproj).
    /// To add OCR later, run it on the PDF where marked below — nothing else changes.
    /// </summary>
    public sealed class ShipFileProcessor : IOcrStarter
    {
        // One file at a time: the ship PC is not a server.
        private static readonly SemaphoreSlim Gate = new(1, 1);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IPdfConversionService _pdfConverter;
        private readonly IPdfDirectTextExtractor _pdfText;
        private readonly IThumbnailGenerator _thumbnails;
        private readonly ILogger<ShipFileProcessor> _logger;

        public ShipFileProcessor(
            IServiceScopeFactory scopeFactory,
            IPdfConversionService pdfConverter,
            IPdfDirectTextExtractor pdfText,
            IThumbnailGenerator thumbnails,
            ILogger<ShipFileProcessor> logger)
        {
            _scopeFactory = scopeFactory;
            _pdfConverter = pdfConverter;
            _pdfText = pdfText;
            _thumbnails = thumbnails;
            _logger = logger;
        }

        public async Task<bool> StartOcr(OcrRequest request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.FilePath))
                return false;

            var fullPath = Path.Combine(ShipDatabase.FilesFolder, request.FilePath);
            if (!File.Exists(fullPath))
            {
                _logger.LogWarning("File processing rejected: file not found | DocumentId: {Id} | File: {File}",
                    request.DocumentId, fullPath);
                return false;
            }

            await Gate.WaitAsync(ct);
            try
            {
                await ProcessAsync(request.DocumentId, fullPath, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "File processing failed | DocumentId: {Id}", request.DocumentId);
            }
            finally
            {
                Gate.Release();
            }

            return true;
        }

        private async Task ProcessAsync(Guid fileGuid, string filePath, CancellationToken ct)
        {
            string? text;
            string? thumbnailPath = null;

            if (OfficeContentExtractor.CanRead(filePath))
            {
                var (officeText, html) = OfficeContentExtractor.Read(filePath);
                await File.WriteAllTextAsync(filePath + ".html", html, ct);
                text = officeText;
            }
            else
            {
                var pdfPath = await _pdfConverter.EnsurePdfAsync(filePath, ct);
                if (pdfPath is null)
                    return;   // no text or preview for this file type

                // ── OCR goes here later: make pdfPath searchable, then read its text below. ──

                text = _pdfText.ExtractText(pdfPath, ct);
                thumbnailPath = await _thumbnails.GenerateAsync(pdfPath, ct);
            }

            if (string.IsNullOrWhiteSpace(text) && thumbnailPath is null)
                return;

            // Own scope: StoredFileService needs IOcrStarter, so it can't be injected here.
            await using var scope = _scopeFactory.CreateAsyncScope();
            var files = scope.ServiceProvider.GetRequiredService<IStoredFileService>();
            await files.UpdateContentFromOCRAsync(fileGuid, text ?? string.Empty, thumbnailPath, ct);
        }
    }
}