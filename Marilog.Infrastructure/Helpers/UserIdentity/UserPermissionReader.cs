using Marilog.Domain.Interfaces.Repositories;
using Marilog.Infrastructure.Models.Identity;
using Marilog.Kernel.Enumerations;
using Microsoft.EntityFrameworkCore;

namespace Marilog.Infrastructure.Helpers.UserIdentity
{
    // Loads a user's effective permissions from the database.
    // Used by PermissionHandler (every request) and AuthService (/me).
    public sealed class UserPermissionReader(IRepository<UserRole> userRoles)
    {
        public async Task<EffectivePermissions> GetAsync(User user, CancellationToken ct = default)
        {
            if (user.IsSuperAdmin) return EffectivePermissions.SuperAdmin;

            var rows = await userRoles.Query()
                .AsNoTracking()
                .Where(ur => ur.UserId == user.Id && ur.Role.IsActive)
                .SelectMany(ur => ur.Role.Permissions)
                .Select(p => new { p.TargetKind, p.TargetValue, p.Actions })
                .ToListAsync(ct);

            return new EffectivePermissions(rows.Select(r => (r.TargetKind, r.TargetValue, r.Actions)));
        }
    }
}