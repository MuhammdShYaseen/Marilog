using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace Marilog.Intelligence.Worker.Services;

public sealed class VectorStoreService
{
    private readonly QdrantClient _client;
    private readonly ILogger<VectorStoreService> _logger;
    private const ulong VectorSize = 1536; // OpenAI text-embedding-3-small

    public VectorStoreService(QdrantClient client, ILogger<VectorStoreService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task EnsureCollectionAsync(string collection, CancellationToken ct = default)
    {
        var collections = await _client.ListCollectionsAsync(ct);
        if (collections.Any(c => c == collection)) return;

        await _client.CreateCollectionAsync(collection,
            new VectorParams { Size = VectorSize, Distance = Distance.Cosine });

        _logger.LogInformation("Collection {Collection} created", collection);
    }

    public async Task UpsertAsync(
        string collection,
        string documentId,
        float[] vector,
        Dictionary<string, string>? metadata,
        CancellationToken ct = default)
    {
        var payload = metadata?
            .ToDictionary(k => k.Key, v => new Value { StringValue = v.Value })
            ?? [];

        payload["document_id"] = new Value { StringValue = documentId };

        var point = new PointStruct
        {
            Id = new PointId { Uuid = documentId },
            Vectors = vector,
            Payload = { payload }
        };

        await _client.UpsertAsync(collection, [point], cancellationToken: ct);
    }

    public async Task<List<ScoredPoint>> SearchAsync(
        string collection,
        float[] queryVector,
        int topK = 5,
        CancellationToken ct = default)
    {
        var results = await _client.SearchAsync(
            collection,
            queryVector,
            limit: (ulong)topK,
            cancellationToken: ct);

        return results.ToList();
    }
}