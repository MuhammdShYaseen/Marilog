using Marilog.Contracts.DTOs.Requests.UserIdenty;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.UserIdenty;
using Marilog.Kernel.Enums;
using System.Net;
using System.Net.Http.Json;

namespace Marilog.Client.Services.UserIdenty
{
    public sealed class AuthService(HttpClient http) : IAuthService
    {
        public async Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken ct = default)
        {
            using var response = await http.PostAsJsonAsync("api/auth/login", request);
            await response.EnsureSuccessWithMessageAsync();
            return LoginResult.Success;
        }

        public async Task LogoutAsync(CancellationToken ct = default)
        {
            using var response = await http.PostAsync("api/auth/logout", null);
            await response.EnsureSuccessWithMessageAsync();
        }

        public async Task<CurrentUserResponse?> GetCurrentUserAsync(CancellationToken ct = default)
        {
            using var response = await http.GetAsync("api/auth/me");
            if (response.StatusCode == HttpStatusCode.Unauthorized) return null;

            await response.EnsureSuccessWithMessageAsync();
            return await response.Content.ReadFromJsonAsync<CurrentUserResponse>();
        }
    }
}