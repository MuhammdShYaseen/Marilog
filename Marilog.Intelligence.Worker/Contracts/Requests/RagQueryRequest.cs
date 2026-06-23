namespace Marilog.Intelligence.Worker.Contracts.Requests
{
    public sealed record RagQueryRequest(
    string Question,
    string Collection,
    int TopK = 5
);
}
