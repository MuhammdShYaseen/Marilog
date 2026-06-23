namespace Marilog.Intelligence.Worker.Contracts.Requests
{
    public sealed record SemanticSearchRequest(
     string Query,
     string Collection,
     int TopK = 5
 );
}
