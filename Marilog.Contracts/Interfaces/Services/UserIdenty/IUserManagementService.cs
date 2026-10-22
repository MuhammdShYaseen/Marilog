using Marilog.Contracts.DTOs.Requests.UserIdenty;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Kernel.Enums;

namespace Marilog.Contracts.Interfaces.Services.UserIdenty
{
    public interface IUserManagementService
    {
        Task<IReadOnlyList<UserResponse>> GetAllAsync(CancellationToken ct = default);
        Task<UserManagementResult> CreateAsync(CreateUserRequest request, CancellationToken ct = default);
        Task<UserManagementResult> ActivateAsync(Guid userId, CancellationToken ct = default);
        Task<UserManagementResult> DeactivateAsync(Guid userId, CancellationToken ct = default);
        Task<UserManagementResult> GrantSuperAdminAsync(Guid userId, CancellationToken ct = default);
        Task<UserManagementResult> RevokeSuperAdminAsync(Guid userId, CancellationToken ct = default);
        Task<UserManagementResult> ResetPasswordAsync(Guid userId, ResetPasswordRequest request, CancellationToken ct = default);
    }
}