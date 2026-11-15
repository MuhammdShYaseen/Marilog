using Marilog.Application.Interfaces.Events;
using Marilog.Application.Interfaces.OCR;
using Marilog.Contracts.DTOs.OCR;
using Marilog.Domain.Events;
using Microsoft.Extensions.Logging;

namespace Marilog.Application.EventHandlers;

public sealed class StoredFileOcrRequestedEventHandler : IEventHandler<StoredFileOcrRequestedEvent>
{
    private readonly ILogger<StoredFileOcrRequestedEventHandler> _logger;
    private readonly IOcrStarter _ocrStarter;

    public StoredFileOcrRequestedEventHandler(
        ILogger<StoredFileOcrRequestedEventHandler> logger,
        IOcrStarter ocrStarter)
    {
        _logger = logger;
        _ocrStarter = ocrStarter;
    }

    public async Task HandleAsync(StoredFileOcrRequestedEvent @event, CancellationToken ct = default)
    {
        _logger.LogInformation("OCR requested | DocumentId: {Id} | File: {File}", @event.StoredFileId, Path.GetFileName(@event.FilePath));

        var request = new OcrRequest
        {
            DocumentId = @event.StoredFileId,
            FilePath = @event.FilePath
        };

        try
        {
            if (await _ocrStarter.StartOcr(request, ct))
                _logger.LogInformation("OCR queued | DocumentId: {Id}", @event.StoredFileId);
            else
                _logger.LogError("OCR could not be queued | DocumentId: {Id}", @event.StoredFileId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OCR starter threw an exception | DocumentId: {Id}", @event.StoredFileId);
        }
    }
}