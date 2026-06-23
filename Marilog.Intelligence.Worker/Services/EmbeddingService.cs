using Marilog.Intelligence.Worker.Models;
using System.Text;
using System.Text.Json;

namespace Marilog.Intelligence.Worker.Services;

public sealed class EmbeddingService
{
    private readonly AiProviderService _providerService;
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<EmbeddingService> _logger;

    public EmbeddingService(
        AiProviderService providerService,
        IHttpClientFactory httpFactory,
        ILogger<EmbeddingService> logger)
    {
        _providerService = providerService;
        _httpFactory = httpFactory;
        _logger = logger;
    }

    public async Task<float[]?> GetEmbeddingAsync(string text, CancellationToken ct = default)
    {
        var provider = await _providerService.GetByTypeAsync(AiProviderType.GoogleGemini, ct);
        if (provider is null)
        {
            _logger.LogError("No embedding provider configured");
            return null;
        }

        var client = _httpFactory.CreateClient();
        client.DefaultRequestHeaders.Add(provider.AuthHeader, $"Bearer {provider.ApiKeyName}");

        // بناء الـ payload من الـ template
        var payload = provider.RequestTemplateJson
            .Replace("{{input}}", text);

        var response = await client.PostAsync(
            provider.BaseUrl,
            new StringContent(payload, Encoding.UTF8, "application/json"),
            ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Embedding API failed: {Status}", response.StatusCode);
            return null;
        }

        var json = await response.Content.ReadFromJsonAsync<JsonDocument>(ct);

        // OpenAI format: data[0].embedding
        var embedding = json?.RootElement
            .GetProperty("data")[0]
            .GetProperty("embedding")
            .Deserialize<float[]>();

        return embedding;
    }
}