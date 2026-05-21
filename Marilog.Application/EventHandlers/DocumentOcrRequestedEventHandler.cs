// Marilog.Application/EventHandlers/DocumentOcrRequestedEventHandler.cs

using Marilog.Application.Interfaces.Events;
using Marilog.Contracts.DTOs.OCR;
using Marilog.Contracts.Interfaces.OCR;
using Marilog.Domain.Events;
using Microsoft.Extensions.Logging;

namespace Marilog.Application.EventHandlers;

public sealed class DocumentOcrRequestedEventHandler : IEventHandler<DocumentOcrRequestedEvent>
{
    private readonly IOcrQueue _ocrQueue;
    private readonly ILogger<DocumentOcrRequestedEventHandler> _logger;

    public DocumentOcrRequestedEventHandler(IOcrQueue ocrQueue, ILogger<DocumentOcrRequestedEventHandler> logger)
    {
        _ocrQueue = ocrQueue;
        _logger = logger;
    }

    public async Task HandleAsync(DocumentOcrRequestedEvent @event, CancellationToken ct = default)
    {
        _logger.LogInformation("OCR requested | DocumentId: {Id} | File: {File}",  @event.DocumentId, Path.GetFileName(@event.FilePath));

        await _ocrQueue.EnqueueAsync(new OcrRequest(@event.DocumentId, @event.FilePath), ct);
    }
}