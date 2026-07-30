using Marilog.Application.Interfaces.Email;
using Marilog.Infrastructure.Models.Email;
using Microsoft.Extensions.Logging;

namespace Marilog.Infrastructure.Services.Email.Google
{
    public sealed class GoogleOAuthTokenService : IGoogleOAuthTokenService
    {
        private static readonly TimeSpan RefreshThreshold = TimeSpan.FromMinutes(2);

        private readonly HttpClient _httpClient;
        private readonly ILogger<GoogleOAuthTokenService> _logger;

        public GoogleOAuthTokenService(
            HttpClient httpClient,
            ILogger<GoogleOAuthTokenService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<bool> EnsureValidAccessTokenAsync(
            Dictionary<string, string> config,
            CancellationToken ct = default)
        {
            if (!config.TryGetValue("accessToken", out var accessToken) ||
                string.IsNullOrWhiteSpace(accessToken))
            {
                throw new InvalidOperationException(
                    "Google account configuration does not contain an accessToken.");
            }

            if (!config.TryGetValue("refreshToken", out var refreshToken) ||
                string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new InvalidOperationException(
                    "Google account configuration does not contain a refreshToken.");
            }

            if (IsAccessTokenValid(config))
                return false;

            var clientId = GetRequired(config, "clientId");
            var clientSecret = GetRequired(config, "clientSecret");

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://oauth2.googleapis.com/token");

            request.Content = new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["client_id"] = clientId,
                    ["client_secret"] = clientSecret,
                    ["refresh_token"] = refreshToken,
                    ["grant_type"] = "refresh_token"
                });

            using var response = await _httpClient.SendAsync(request, ct);

            var body = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Google OAuth token refresh failed. Status: {StatusCode}, Response: {Response}",
                    response.StatusCode,
                    body);

                throw new InvalidOperationException(
                    $"Google OAuth token refresh failed: {response.StatusCode}");
            }

            var tokenResponse =
                System.Text.Json.JsonSerializer.Deserialize<GoogleTokenResponse>(body);

            if (tokenResponse is null ||
                string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
            {
                throw new InvalidOperationException(
                    "Google OAuth token endpoint returned an invalid response.");
            }

            config["accessToken"] = tokenResponse.AccessToken;

            var expiresIn = tokenResponse.ExpiresIn > 0
                ? tokenResponse.ExpiresIn
                : 3600;

            config["expiresAt"] =
                DateTime.UtcNow.AddSeconds(expiresIn).ToString("O");

            // Google normally does NOT return a new refresh token
            // during a normal refresh flow.
            if (!string.IsNullOrWhiteSpace(tokenResponse.RefreshToken))
                config["refreshToken"] = tokenResponse.RefreshToken;

            return true;
        }

        private static bool IsAccessTokenValid(
            Dictionary<string, string> config)
        {
            if (!config.TryGetValue("expiresAt", out var value))
                return false;

            if (!DateTime.TryParse(
                    value,
                    null,
                    System.Globalization.DateTimeStyles.RoundtripKind,
                    out var expiresAt))
            {
                return false;
            }

            return expiresAt.ToUniversalTime() >
                   DateTime.UtcNow.Add(RefreshThreshold);
        }

        private static string GetRequired(
            Dictionary<string, string> config,
            string key)
        {
            if (!config.TryGetValue(key, out var value) ||
                string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException(
                    $"Email configuration is missing '{key}'.");
            }

            return value;
        }
    }
}
