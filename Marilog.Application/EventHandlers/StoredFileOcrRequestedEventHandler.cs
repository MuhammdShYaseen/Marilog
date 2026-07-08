using Marilog.Application.Interfaces.Events;
using Marilog.Contracts.DTOs.OCR;
using Marilog.Contracts.Options;
using Marilog.Domain.Entities.SystemEntities;
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
    private readonly IOptions<InternalApiKeysOptions> _apiKeysOptions;
    public StoredFileOcrRequestedEventHandler(ILogger<StoredFileOcrRequestedEventHandler> logger, HttpClient httpClient, IOptions<UrlsOptions> urlOptions, IOptions<InternalApiKeysOptions> apiKeyOptions)
    {
        _logger = logger;
        _httpClient = httpClient;
        _urlsOptions = urlOptions;
        _apiKeysOptions = apiKeyOptions;
    }

    public async Task HandleAsync(StoredFileOcrRequestedEvent @event, CancellationToken ct = default)
    {
        _logger.LogInformation("OCR requested | DocumentId: {Id} | File: {File}", @event.StoredFileId, Path.GetFileName(@event.FilePath));

        var request = new OcrRequest 
        {
            DocumentId =@event.StoredFileId,
            FilePath = @event.FilePath
        };

        try
        {
            using var message = new HttpRequestMessage(HttpMethod.Post, _urlsOptions.Value.Ocr + "api/ocr/process")
            {
                Content = JsonContent.Create(request)
            };

            message.Headers.Add("X-Internal-Api-Key", _apiKeysOptions.Value.OcrWorkerKey); // ← عدّل اسم الـ header هون إذا مختلف

            var response = await _httpClient.SendAsync(message, ct);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(ct);

                _logger.LogError(
                    "OCR worker request failed | Status: {Status} | Error: {Error}",
                    response.StatusCode,
                    error);

                response.EnsureSuccessStatusCode();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to call OCR worker | DocumentId: {Id}", @event.StoredFileId);
        }

        _logger.LogInformation("OCR worker accepted request | DocumentId: {Id}", @event.StoredFileId);
    }
}