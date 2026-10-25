using Marilog.Contracts.DTOs.Requests.UserIdenty;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.UserIdenty;
using Marilog.Kernel.Enums;
using System.Net.Http.Json;

namespace Marilog.Client.Services.UserIdenty
{
    public class RoleManagementService : IRoleManagementService
    {
        private readonly HttpClient _http;
        private const string Base = "api/roles";
        private const string UsersBase = "api/users";

        public RoleManagementService(HttpClient http) => _http = http;

        public async Task<IReadOnlyList<RoleResponse>> GetAllAsync(CancellationToken ct = default)
        {
            var roles = await _http.GetCheckedAsync<List<RoleResponse>>(Base, ct);
            return roles ?? [];
        }

        public async Task<IReadOnlyList<RolePermissionResponse>?> GetPermissionsAsync(int roleId, CancellationToken ct = default)
            => await _http.GetCheckedAsync<List<RolePermissionResponse>>($"{Base}/{roleId}/permissions", ct);

        public async Task<RoleManagementResult> CreateAsync(CreateRoleRequest request, CancellationToken ct = default)
        {
            var http = await _http.PostAsJsonAsync(Base, request, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
            return RoleManagementResult.Success;
        }

        public async Task<RoleManagementResult> UpdateAsync(int roleId, UpdateRoleRequest request, CancellationToken ct = default)
        {
            var http = await _http.PutAsJsonAsync($"{Base}/{roleId}", request, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
            return RoleManagementResult.Success;
        }

        public Task<RoleManagementResult> ActivateAsync(int roleId, CancellationToken ct = default)
            => PostActionAsync(roleId, "activate", ct);

        public Task<RoleManagementResult> DeactivateAsync(int roleId, CancellationToken ct = default)
            => PostActionAsync(roleId, "deactivate", ct);

        public async Task<RoleManagementResult> DeleteAsync(int roleId, CancellationToken ct = default)
        {
            var http = await _http.DeleteAsync($"{Base}/{roleId}", ct);
            await http.EnsureSuccessWithMessageAsync(ct);
            return RoleManagementResult.Success;
        }

        public async Task<RoleManagementResult> SetPermissionsAsync(int roleId, IReadOnlyList<RolePermissionRequest> permissions, CancellationToken ct = default)
        {
            var http = await _http.PutAsJsonAsync($"{Base}/{roleId}/permissions", permissions, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
            return RoleManagementResult.Success;
        }

        public async Task<RoleManagementResult> SetUserRolesAsync(Guid userId, IReadOnlyList<int> roleIds, CancellationToken ct = default)
        {
            var http = await _http.PutAsJsonAsync($"{UsersBase}/{userId}/roles", roleIds, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
            return RoleManagementResult.Success;
        }

        // Failures throw with the server's message
        private async Task<RoleManagementResult> PostActionAsync(int roleId, string action, CancellationToken ct)
        {
            var http = await _http.PostAsync($"{Base}/{roleId}/{action}", null, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
            return RoleManagementResult.Success;
        }
    }
}