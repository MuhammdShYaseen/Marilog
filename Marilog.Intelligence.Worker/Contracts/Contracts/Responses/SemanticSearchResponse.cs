namespace Marilog.Intelligence.Worker.Contracts.Contracts.Responses
{
    public sealed record SemanticSearchResponse(
    List<SearchResult> Results
);

    public sealed record SearchResult(
        string DocumentId,
        float Score,
        Dictionary<string, string> Metadata
    );
}
