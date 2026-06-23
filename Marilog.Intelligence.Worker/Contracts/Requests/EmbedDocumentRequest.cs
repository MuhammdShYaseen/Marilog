namespace Marilog.Intelligence.Worker.Contracts.Requests
{
    public sealed record EmbedDocumentRequest(
    string DocumentId,
    string Text,
    string Collection,
    Dictionary<string, string>? Metadata = null
);
}
