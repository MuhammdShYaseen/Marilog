namespace Marilog.Intelligence.Worker.Contracts.Responses
{
    public sealed record EmbedDocumentResponse(
     string DocumentId,
     bool Success,
     string? Error = null
 );
}
