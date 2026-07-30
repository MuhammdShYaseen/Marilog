namespace Marilog.Application.Interfaces.Email
{
    public interface IGoogleOAuthTokenService
    {
        Task<bool> EnsureValidAccessTokenAsync(Dictionary<string, string> config, CancellationToken ct = default);
    } 
}
