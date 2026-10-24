using Marilog.Contracts.DTOs.Requests.UserIdenty;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Kernel.Enums;

namespace Marilog.Contracts.Interfaces.Services.UserIdenty
{
    public interface IRoleManagementService
    {
        Task<IReadOnlyList<RoleResponse>> GetAllAsync(CancellationToken ct = default);

        // Null when the role doesn't exist.
        Task<IReadOnlyList<RolePermissionResponse>?> GetPermissionsAsync(int roleId, CancellationToken ct = default);

        Task<RoleManagementResult> CreateAsync(CreateRoleRequest request, CancellationToken ct = default);
        Task<RoleManagementResult> UpdateAsync(int roleId, UpdateRoleRequest request, CancellationToken ct = default);
        Task<RoleManagementResult> ActivateAsync(int roleId, CancellationToken ct = default);
        Task<RoleManagementResult> DeactivateAsync(int roleId, CancellationToken ct = default);
        Task<RoleManagementResult> DeleteAsync(int roleId, CancellationToken ct = default);

        // Replaces the role's full permission set.
        Task<RoleManagementResult> SetPermissionsAsync(int roleId, IReadOnlyList<RolePermissionRequest> permissions, CancellationToken ct = default);

        // Replaces the user's full role set.
        Task<RoleManagementResult> SetUserRolesAsync(Guid userId, IReadOnlyList<int> roleIds, CancellationToken ct = default);
    }
}