using Marilog.Application.Interfaces.Events;
using Marilog.Contracts.DTOs.OCR;
using Marilog.Domain.Events;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace Marilog.Application.EventHandlers;

public sealed class DocumentOcrRequestedEventHandler : IEventHandler<DocumentOcrRequestedEvent>
{
    private readonly ILogger<DocumentOcrRequestedEventHandler> _logger;
    private readonly HttpClient _httpClient;
    public DocumentOcrRequestedEventHandler(ILogger<DocumentOcrRequestedEventHandler> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task HandleAsync(DocumentOcrRequestedEvent @event, CancellationToken ct = default)
    {
        _logger.LogInformation("OCR requested | DocumentId: {Id} | File: {File}", @event.DocumentId, Path.GetFileName(@event.FilePath));

        var request = new OcrRequest(
            @event.DocumentId,
            @event.FilePath);

        var response = await _httpClient.PostAsJsonAsync("/api/ocr/process", request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);

            _logger.LogError(
                "OCR worker request failed | Status: {Status} | Error: {Error}",
                response.StatusCode,
                error);

            response.EnsureSuccessStatusCode();
        }

        _logger.LogInformation("OCR worker accepted request | DocumentId: {Id}", @event.DocumentId);
    }
}