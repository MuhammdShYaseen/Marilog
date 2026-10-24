
namespace Marilog.Contracts.DTOs.Responses
{
    public sealed record UserResponse(
       Guid Id,
       string UserName,
       string FullName,
       string? Email,
       bool IsActive,
       bool IsSuperAdmin,
       bool IsLockedOut,
       IReadOnlyList<int> RoleIds);
}
