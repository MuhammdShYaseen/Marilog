using Marilog.Kernel.Enums;

namespace Marilog.Contracts.DTOs.Responses
{
    public sealed record RolePermissionResponse(PermissionTargetKind Kind, int Value, PermissionAction Actions);
}