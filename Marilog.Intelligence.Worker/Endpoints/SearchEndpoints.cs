using Marilog.Intelligence.Worker.Contracts.Requests;
using Marilog.Intelligence.Worker.Services;

namespace Marilog.Intelligence.Worker.Endpoints;

public static class SearchEndpoints
{
    public static void MapSearchEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/search").WithTags("Search");

        group.MapPost("/", async (
            SemanticSearchRequest request,
            SemanticSearchService searchService,
            CancellationToken ct) =>
        {
            var result = await searchService.SearchAsync(request, ct);
            return Results.Ok(result);
        });
    }
}