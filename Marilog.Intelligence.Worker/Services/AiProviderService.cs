using Marilog.Intelligence.Worker.Models;

namespace Marilog.Intelligence.Worker.Services;

public sealed class AiProviderService
{
    private readonly HttpClient _http;
    private readonly ILogger<AiProviderService> _logger;

    public AiProviderService(HttpClient http, ILogger<AiProviderService> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<AiProviderResponse?> GetByTypeAsync(AiProviderType providerType, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.GetFromJsonAsync<List<AiProviderResponse>>(
                $"/api/ai-providers/by-type/{providerType}", ct);

            return response?.FirstOrDefault(p =>
                p.ProviderType == providerType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch AiProvider of type {Type}", providerType);
            return null;
        }
    }
}