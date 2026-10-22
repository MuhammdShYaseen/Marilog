using Marilog.Contracts.DTOs.Requests.UserIdenty;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.UserIdenty;
using Marilog.Kernel.Enums;
using System.Net.Http.Json;

namespace Marilog.Client.Services.UserIdenty
{
    public class UserManagementService : IUserManagementService
    {
        private readonly HttpClient _http;
        private const string Base = "api/users";

        public UserManagementService(HttpClient http) => _http = http;

        public async Task<IReadOnlyList<UserResponse>> GetAllAsync(CancellationToken ct = default)
        {
            var users = await _http.GetCheckedAsync<List<UserResponse>>(Base, ct);
            return users ?? [];
        }

        public async Task<UserManagementResult> CreateAsync(CreateUserRequest request, CancellationToken ct = default)
        {
            var http = await _http.PostAsJsonAsync(Base, request, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
            return UserManagementResult.Success;
        }

        public Task<UserManagementResult> ActivateAsync(Guid userId, CancellationToken ct = default)
            => PostActionAsync(userId, "activate", ct);

        public Task<UserManagementResult> DeactivateAsync(Guid userId, CancellationToken ct = default)
            => PostActionAsync(userId, "deactivate", ct);

        public Task<UserManagementResult> GrantSuperAdminAsync(Guid userId, CancellationToken ct = default)
            => PostActionAsync(userId, "grant-super-admin", ct);

        public Task<UserManagementResult> RevokeSuperAdminAsync(Guid userId, CancellationToken ct = default)
            => PostActionAsync(userId, "revoke-super-admin", ct);

        public async Task<UserManagementResult> ResetPasswordAsync(Guid userId, ResetPasswordRequest request, CancellationToken ct = default)
        {
            var http = await _http.PostAsJsonAsync($"{Base}/{userId}/reset-password", request, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
            return UserManagementResult.Success;
        }

        // Failures throw with the server's message, same as the audit service
        private async Task<UserManagementResult> PostActionAsync(Guid userId, string action, CancellationToken ct)
        {
            var http = await _http.PostAsync($"{Base}/{userId}/{action}", null, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
            return UserManagementResult.Success;
        }
    }
}