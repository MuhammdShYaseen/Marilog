using Marilog.Kernel.Enumerations;
using Marilog.Kernel.Enums;
using System.Globalization;
using System.Security.Claims;

public static class PermissionClaims
{
    public const string ClaimType = "perm";
    public const string SuperAdminRole = "SuperAdmin";

    public static string ToValue(PermissionTargetKind kind, int value, PermissionAction actions)
        => string.Create(CultureInfo.InvariantCulture, $"{(int)kind}:{value}:{(int)actions}");

    public static EffectivePermissions GetPermissions(this ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true) return EffectivePermissions.None;
        if (principal.IsInRole(SuperAdminRole)) return EffectivePermissions.SuperAdmin;

        var grants = new List<(PermissionTargetKind, int, PermissionAction)>();
        foreach (var claim in principal.FindAll(ClaimType))
        {
            var parts = claim.Value.Split(':');
            if (parts.Length == 3
                && int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var kind)
                && int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
                && int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var actions))
                grants.Add(((PermissionTargetKind)kind, value, (PermissionAction)actions));
        }

        return new EffectivePermissions(grants);
    }

    public static bool Can(this ClaimsPrincipal principal, EntityType entity, PermissionAction action)
        => principal.GetPermissions().Can(entity, action);

    public static bool Can(this ClaimsPrincipal principal, AppScreen screen)
        => principal.GetPermissions().Can(screen);
}