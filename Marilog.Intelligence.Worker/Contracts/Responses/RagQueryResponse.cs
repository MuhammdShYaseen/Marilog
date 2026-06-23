namespace Marilog.Intelligence.Worker.Contracts.Responses
{
    public sealed record RagQueryResponse(
    string Answer,
    List<string> SourceDocumentIds
);
}
