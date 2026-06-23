using Marilog.Intelligence.Worker.Contracts.Contracts.Responses;
using Marilog.Intelligence.Worker.Contracts.Requests;

namespace Marilog.Intelligence.Worker.Services;

public sealed class SemanticSearchService
{
    private readonly EmbeddingService _embedding;
    private readonly VectorStoreService _vectorStore;

    public SemanticSearchService(EmbeddingService embedding, VectorStoreService vectorStore)
    {
        _embedding = embedding;
        _vectorStore = vectorStore;
    }

    public async Task<SemanticSearchResponse> SearchAsync(
        SemanticSearchRequest request,
        CancellationToken ct = default)
    {
        var queryVector = await _embedding.GetEmbeddingAsync(request.Query, ct);
        if (queryVector is null)
            return new SemanticSearchResponse([]);

        var points = await _vectorStore.SearchAsync(request.Collection, queryVector, request.TopK, ct);

        var results = points.Select(p => new SearchResult(
            DocumentId: p.Payload["document_id"].StringValue,
            Score: p.Score,
            Metadata: p.Payload
                .Where(kv => kv.Key != "document_id")
                .ToDictionary(kv => kv.Key, kv => kv.Value.StringValue)
        )).ToList();

        return new SemanticSearchResponse(results);
    }
}