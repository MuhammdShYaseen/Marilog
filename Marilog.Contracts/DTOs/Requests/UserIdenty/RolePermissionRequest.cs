using Marilog.Kernel.Enums;

namespace Marilog.Contracts.DTOs.Requests.UserIdenty
{
    public sealed record RolePermissionRequest(PermissionTargetKind Kind, int Value, PermissionAction Actions);
}