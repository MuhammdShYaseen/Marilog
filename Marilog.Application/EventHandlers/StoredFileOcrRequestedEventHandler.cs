using Marilog.Application.Interfaces.Events;
using Marilog.Contracts.DTOs.OCR;
using Marilog.Contracts.Options;
using Marilog.Domain.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace Marilog.Application.EventHandlers;

public sealed class StoredFileOcrRequestedEventHandler : IEventHandler<StoredFileOcrRequestedEvent>
{
    private readonly ILogger<StoredFileOcrRequestedEventHandler> _logger;
    private readonly HttpClient _httpClient;
    private readonly IOptions<UrlsOptions> _urlsOptions;
    public StoredFileOcrRequestedEventHandler(ILogger<StoredFileOcrRequestedEventHandler> logger, HttpClient httpClient, IOptions<UrlsOptions> urlOptions)
    {
        _logger = logger;
        _httpClient = httpClient;
        _urlsOptions = urlOptions;
    }

    public async Task HandleAsync(StoredFileOcrRequestedEvent @event, CancellationToken ct = default)
    {
        _logger.LogInformation("OCR requested | DocumentId: {Id} | File: {File}", @event.StoredFileId, Path.GetFileName(@event.FilePath));

        var request = new OcrRequest(
            @event.StoredFileId,
            @event.FilePath);

        var response = await _httpClient.PostAsJsonAsync(_urlsOptions.Value.Ocr + "api/ocr/process", request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);

            _logger.LogError(
                "OCR worker request failed | Status: {Status} | Error: {Error}",
                response.StatusCode,
                error);

            response.EnsureSuccessStatusCode();
        }

        _logger.LogInformation("OCR worker accepted request | DocumentId: {Id}", @event.StoredFileId);
    }
}