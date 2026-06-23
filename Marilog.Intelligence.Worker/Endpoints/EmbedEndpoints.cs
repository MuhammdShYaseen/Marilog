using Marilog.Intelligence.Worker.Contracts.Requests;
using Marilog.Intelligence.Worker.Contracts.Responses;
using Marilog.Intelligence.Worker.Services;

namespace Marilog.Intelligence.Worker.Endpoints;

public static class EmbedEndpoints
{
    public static void MapEmbedEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/embed").WithTags("Embedding");

        group.MapPost("/", async (
            EmbedDocumentRequest request,
            EmbeddingService embeddingService,
            VectorStoreService vectorStore,
            CancellationToken ct) =>
        {
            await vectorStore.EnsureCollectionAsync(request.Collection, ct);

            var vector = await embeddingService.GetEmbeddingAsync(request.Text, ct);
            if (vector is null)
                return Results.Ok(new EmbedDocumentResponse(request.DocumentId, false, "Embedding failed"));

            await vectorStore.UpsertAsync(
                request.Collection,
                request.DocumentId,
                vector,
                request.Metadata,
                ct);

            return Results.Ok(new EmbedDocumentResponse(request.DocumentId, true));
        });
    }
}