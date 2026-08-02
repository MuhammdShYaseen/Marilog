using Marilog.Application.Interfaces.Email;
using Marilog.Application.Models;
using Marilog.Contracts.Options;
using Marilog.Infrastructure.Models.Email;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text.Json;

namespace Marilog.Infrastructure.Services.Email.Google
{
    public sealed class GoogleOAuthTokenService : IGoogleOAuthTokenService
    {
        private static readonly TimeSpan RefreshThreshold = TimeSpan.FromMinutes(2);

        private readonly HttpClient _httpClient;
        private readonly ILogger<GoogleOAuthTokenService> _logger;
        private readonly GoogleOAuthOptions _options;
        public GoogleOAuthTokenService(HttpClient httpClient, IOptions<GoogleOAuthOptions> options, ILogger<GoogleOAuthTokenService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _options = options.Value;
        }

        public string GetAuthorizationUrl()
        {
            var query = new Dictionary<string, string>
            {
                ["client_id"] = _options.ClientID!,
                ["redirect_uri"] = _options.RedirectUri!,
                ["response_type"] = "code",
                ["scope"] =
                    "https://www.googleapis.com/auth/gmail.modify " +
                    "https://www.googleapis.com/auth/gmail.send",
                ["access_type"] = "offline",
                ["prompt"] = "consent",
                ["include_granted_scopes"] = "true"
            };

            var queryString = string.Join(
                "&",
                query.Select(x =>
                    $"{WebUtility.UrlEncode(x.Key)}={WebUtility.UrlEncode(x.Value)}"));

            return $"https://accounts.google.com/o/oauth2/v2/auth?{queryString}";
        }

        public async Task<GoogleTokenResponse> ExchangeCodeForTokenAsync(
     string authorizationCode,
     CancellationToken ct = default)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://oauth2.googleapis.com/token");

            request.Content = new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["code"] = authorizationCode,
                    ["client_id"] = _options.ClientID!,
                    ["client_secret"] = _options.ClientSecret!,
                    ["redirect_uri"] = _options.RedirectUri!,
                    ["grant_type"] = "authorization_code"
                });

            using var response = await _httpClient.SendAsync(request, ct);

            var body = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Google OAuth authorization failed. Status: {StatusCode}, Response: {Response}",
                    response.StatusCode,
                    body);

                throw new InvalidOperationException(
                    $"Google OAuth authorization failed: {response.StatusCode}");
            }

            var token = JsonSerializer.Deserialize<GoogleTokenResponse>(body);

            if (token is null || string.IsNullOrWhiteSpace(token.AccessToken))
            {
                throw new InvalidOperationException(
                    "Google OAuth returned an invalid token response.");
            }

            return token;
        }

        public async Task<bool> EnsureValidAccessTokenAsync(Dictionary<string, string> config, CancellationToken ct = default)
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
                    ["client_id"] = _options.ClientID ?? "",
                    ["client_secret"] = _options.ClientSecret ?? "",
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
