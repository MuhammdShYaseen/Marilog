using Marilog.Intelligence.Worker.Contracts.Requests;
using Marilog.Intelligence.Worker.Contracts.Responses;
using Marilog.Intelligence.Worker.Models;
using System.Text;
using System.Text.Json;

namespace Marilog.Intelligence.Worker.Services;

public sealed class RagService
{
    private readonly SemanticSearchService _search;
    private readonly AiProviderService _providerService;
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<RagService> _logger;

    public RagService(
        SemanticSearchService search,
        AiProviderService providerService,
        IHttpClientFactory httpFactory,
        ILogger<RagService> logger)
    {
        _search = search;
        _providerService = providerService;
        _httpFactory = httpFactory;
        _logger = logger;
    }

    public async Task<RagQueryResponse> QueryAsync(RagQueryRequest request, CancellationToken ct = default)
    {
        // 1. ابحث عن المستندات ذات الصلة
        var searchResult = await _search.SearchAsync(
            new SemanticSearchRequest(request.Question, request.Collection, request.TopK), ct);

        if (searchResult.Results.Count == 0)
            return new RagQueryResponse("No relevant documents found.", []);

        // 2. احضر الـ chat provider
        var provider = await _providerService.GetByTypeAsync(AiProviderType.OpenAI, ct);
        if (provider is null)
            return new RagQueryResponse("Chat provider not configured.", []);

        // 3. ابني الـ context من نتائج البحث
        var context = string.Join("\n\n", searchResult.Results
            .Select((r, i) => $"[{i + 1}] {r.Metadata.GetValueOrDefault("text", "")}"));

        var prompt = $"""
            You are a maritime operations assistant for WKA Ocean Shipping.
            Answer the question based only on the provided context.
            If the answer is not in the context, say "I don't have enough information."

            Context:
            {context}

            Question: {request.Question}
            """;

        // 4. استدعي الـ chat API
        var payload = provider.RequestTemplateJson
            .Replace("{{prompt}}", prompt);

        var client = _httpFactory.CreateClient();
        client.DefaultRequestHeaders.Add(provider.AuthHeader, $"Bearer {provider.ApiKeyEncrypted}");

        var response = await client.PostAsync(
            provider.BaseUrl,
            new StringContent(payload, Encoding.UTF8, "application/json"),
            ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Chat API failed: {Status}", response.StatusCode);
            return new RagQueryResponse("Failed to generate answer.", []);
        }

        var json = await response.Content.ReadFromJsonAsync<JsonDocument>(ct);
        var answer = json?.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? string.Empty;

        var sourceIds = searchResult.Results.Select(r => r.DocumentId).ToList();

        return new RagQueryResponse(answer, sourceIds);
    }
}