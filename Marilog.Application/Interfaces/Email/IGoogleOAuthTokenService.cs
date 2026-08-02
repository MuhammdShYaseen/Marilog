using Marilog.Application.Models;

namespace Marilog.Application.Interfaces.Email
{
    public interface IGoogleOAuthTokenService
    {

        public string GetAuthorizationUrl(string state);
        Task<GoogleTokenResponse> ExchangeCodeForTokenAsync(string authorizationCode, CancellationToken ct = default);
        Task<bool> EnsureValidAccessTokenAsync(Dictionary<string, string> config, CancellationToken ct = default);
    } 
}
