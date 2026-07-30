using Marilog.Application.Interfaces.Events;
using Marilog.Application.Interfaces.OCR;
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
    private readonly IOptions<InternalApiKeysOptions> _apiKeysOptions;
    private readonly IOcrStarter _ocrStarter;

    public StoredFileOcrRequestedEventHandler(
        ILogger<StoredFileOcrRequestedEventHandler> logger,
        HttpClient httpClient,
        IOptions<UrlsOptions> urlOptions,
        IOptions<InternalApiKeysOptions> apiKeyOptions,
        IOcrStarter ocrStarter)
    {
        _logger = logger;
        _httpClient = httpClient;
        _urlsOptions = urlOptions;
        _apiKeysOptions = apiKeyOptions;
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

        // 1) المسار الداخلي (in-process queue) أولاً
        var startedLocally = await TryStartLocalAsync(request, ct);

        if (startedLocally)
        {
            _logger.LogInformation("OCR started via local worker | DocumentId: {Id}", @event.StoredFileId);
            return;
        }

        _logger.LogWarning("Local OCR starter failed, falling back to HTTP | DocumentId: {Id}", @event.StoredFileId);

        // 2) fallback عبر HTTP
        await TryStartViaHttpAsync(request, @event, ct);
    }

    private async Task<bool> TryStartLocalAsync(OcrRequest request, CancellationToken ct)
    {
        try
        {
            return await _ocrStarter.StartOcr(request, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Local OCR starter threw an exception | DocumentId: {Id}", request.DocumentId);
            return false;
        }
    }

    private async Task TryStartViaHttpAsync(OcrRequest request, StoredFileOcrRequestedEvent @event, CancellationToken ct)
    {
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

            _logger.LogInformation("OCR worker accepted request via HTTP | DocumentId: {Id}", @event.StoredFileId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to call OCR worker via HTTP | DocumentId: {Id}", @event.StoredFileId);
        }
    }
}