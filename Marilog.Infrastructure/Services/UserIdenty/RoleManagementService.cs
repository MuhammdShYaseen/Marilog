using Marilog.Contracts.DTOs.Requests.UserIdenty;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.UserIdenty;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Infrastructure.Models.Identity;
using Marilog.Kernel.Enumerations;
using Marilog.Kernel.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Marilog.Infrastructure.Services.UserIdenty
{
    public sealed class RoleManagementService(
        IRepository<Role> roles,
        IRepository<UserRole> userRoles,
        UserManager<User> userManager) : IRoleManagementService
    {
        public async Task<IReadOnlyList<RoleResponse>> GetAllAsync(CancellationToken ct = default)
        {
            var userCounts = await userRoles.Query()
                .AsNoTracking()
                .GroupBy(ur => ur.RoleId)
                .Select(g => new { RoleId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.RoleId, x => x.Count, ct);

            var list = await roles.Query()
                .AsNoTracking()
                .OrderBy(r => r.Name)
                .Select(r => new { r.Id, r.Name, r.Description, r.IsActive })
                .ToListAsync(ct);

            return list
                .Select(r => new RoleResponse(r.Id, r.Name, r.Description, r.IsActive, userCounts.GetValueOrDefault(r.Id)))
                .ToList();
        }

        public async Task<IReadOnlyList<RolePermissionResponse>?> GetPermissionsAsync(int roleId, CancellationToken ct = default)
        {
            if (!await roles.Query().AnyAsync(r => r.Id == roleId, ct))
                return null;

            return await roles.Query()
                .AsNoTracking()
                .Where(r => r.Id == roleId)
                .SelectMany(r => r.Permissions)
                .OrderBy(p => p.TargetKind)
                .ThenBy(p => p.TargetValue)
                .Select(p => new RolePermissionResponse(p.TargetKind, p.TargetValue, p.Actions))
                .ToListAsync(ct);
        }

        public async Task<RoleManagementResult> CreateAsync(CreateRoleRequest request, CancellationToken ct = default)
        {
            var name = request.Name?.Trim();
            if (string.IsNullOrWhiteSpace(name)) return RoleManagementResult.InvalidName;
            if (await IsNameTakenAsync(name, excludeId: null, ct)) return RoleManagementResult.NameTaken;

            await roles.AddAsync(Role.Create(name, request.Description), ct);
            await roles.SaveChangesAsync(ct);
            return RoleManagementResult.Success;
        }

        public async Task<RoleManagementResult> UpdateAsync(int roleId, UpdateRoleRequest request, CancellationToken ct = default)
        {
            var name = request.Name?.Trim();
            if (string.IsNullOrWhiteSpace(name)) return RoleManagementResult.InvalidName;

            var role = await roles.Query().FirstOrDefaultAsync(r => r.Id == roleId, ct);
            if (role is null) return RoleManagementResult.NotFound;
            if (await IsNameTakenAsync(name, excludeId: roleId, ct)) return RoleManagementResult.NameTaken;

            role.Update(name, request.Description);
            await roles.SaveChangesAsync(ct);
            return RoleManagementResult.Success;
        }

        public async Task<RoleManagementResult> ActivateAsync(int roleId, CancellationToken ct = default)
        {
            var role = await roles.Query().FirstOrDefaultAsync(r => r.Id == roleId, ct);
            if (role is null) return RoleManagementResult.NotFound;

            role.Activate();
            await roles.SaveChangesAsync(ct);
            return RoleManagementResult.Success;
        }

        // Takes effect on the next request: the permission handler ignores inactive roles.
        public async Task<RoleManagementResult> DeactivateAsync(int roleId, CancellationToken ct = default)
        {
            var role = await roles.Query().FirstOrDefaultAsync(r => r.Id == roleId, ct);
            if (role is null) return RoleManagementResult.NotFound;

            role.Deactivate();
            await roles.SaveChangesAsync(ct);
            return RoleManagementResult.Success;
        }

        public async Task<RoleManagementResult> DeleteAsync(int roleId, CancellationToken ct = default)
        {
            // Permissions are loaded so EF deletes them as tracked entities and the audit records each one.
            var role = await roles.Query()
                .Include(r => r.Permissions)
                .FirstOrDefaultAsync(r => r.Id == roleId, ct);
            if (role is null) return RoleManagementResult.NotFound;

            if (await userRoles.Query().AnyAsync(ur => ur.RoleId == roleId, ct))
                return RoleManagementResult.RoleInUse;

            roles.HardDelete(role);
            await roles.SaveChangesAsync(ct);
            return RoleManagementResult.Success;
        }

        public async Task<RoleManagementResult> SetPermissionsAsync(
            int roleId, IReadOnlyList<RolePermissionRequest> permissions, CancellationToken ct = default)
        {
            var role = await roles.Query()
                .Include(r => r.Permissions)
                .FirstOrDefaultAsync(r => r.Id == roleId, ct);
            if (role is null) return RoleManagementResult.NotFound;

            // Normalize every item through the catalog (drops unsupported actions, forces Read with writes)
            // and merge duplicate targets.
            var desired = new Dictionary<(PermissionTargetKind Kind, int Value), PermissionAction>();
            foreach (var item in permissions)
            {
                var actions = PermissionCatalog.Normalize(item.Kind, item.Value, item.Actions);
                if (actions == PermissionAction.None) continue;

                var key = (item.Kind, item.Value);
                desired[key] = desired.TryGetValue(key, out var existing) ? existing | actions : actions;
            }

            // Diff against the stored rows, so the audit shows only what really changed.
            foreach (var current in role.Permissions.ToList())
            {
                if (desired.Remove((current.TargetKind, current.TargetValue), out var actions))
                {
                    if (current.Actions != actions)
                        current.SetActions(actions);
                }
                else
                {
                    role.Permissions.Remove(current);
                }
            }

            foreach (var ((kind, value), actions) in desired)
                role.Permissions.Add(RolePermission.Create(kind, value, actions));

            // No Update() call: the role is tracked, and Update() would mark every row as modified.
            await roles.SaveChangesAsync(ct);
            return RoleManagementResult.Success;
        }

        public async Task<RoleManagementResult> SetUserRolesAsync(
            Guid userId, IReadOnlyList<int> roleIds, CancellationToken ct = default)
        {
            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user is null) return RoleManagementResult.NotFound;

            var desired = roleIds.ToHashSet();
            if (desired.Count > 0)
            {
                var found = await roles.Query().CountAsync(r => desired.Contains(r.Id), ct);
                if (found != desired.Count) return RoleManagementResult.InvalidRole;
            }

            var current = await userRoles.Query()
                .Where(ur => ur.UserId == userId)
                .ToListAsync(ct);

            var toRemove = new List<UserRole>();
            foreach (var userRole in current)
            {
                if (!desired.Remove(userRole.RoleId))
                    toRemove.Add(userRole);
            }

            if (toRemove.Count > 0)
                userRoles.RemoveRange(toRemove);

            if (desired.Count > 0)
                await userRoles.AddRangeAsync(desired.Select(roleId => UserRole.Create(userId, roleId)), ct);

            await userRoles.SaveChangesAsync(ct);
            return RoleManagementResult.Success;
        }

        private Task<bool> IsNameTakenAsync(string name, int? excludeId, CancellationToken ct)
            => roles.Query().AnyAsync(r => r.Name == name && (excludeId == null || r.Id != excludeId), ct);
    }
}