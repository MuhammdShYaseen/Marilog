using Marilog.Intelligence.Worker.Contracts.Requests;
using Marilog.Intelligence.Worker.Services;

namespace Marilog.Intelligence.Worker.Endpoints;

public static class RagEndpoints
{
    public static void MapRagEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/rag").WithTags("RAG");

        group.MapPost("/", async (
            RagQueryRequest request,
            RagService ragService,
            CancellationToken ct) =>
        {
            var result = await ragService.QueryAsync(request, ct);
            return Results.Ok(result);
        });
    }
}