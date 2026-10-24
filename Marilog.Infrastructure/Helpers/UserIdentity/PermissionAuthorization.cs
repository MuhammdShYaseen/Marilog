using Marilog.Infrastructure.DataAccess.ContextDb;
using Marilog.Infrastructure.Models.Identity;
using Marilog.Kernel.Enumerations;
using Marilog.Kernel.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Marilog.Infrastructure.Helpers.UserIdentity
{
    public sealed record PermissionRequirement(PermissionTargetKind Kind, int Value, PermissionAction Action)
        : IAuthorizationRequirement;

    // Usage: [RequirePermission(EntityType.Vessel, PermissionAction.Create)] or [RequirePermission(AppScreen.FinancialDashboard)].
    // Derives from AuthorizeAttribute, so an authenticated user is always required as well.
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public sealed class RequirePermissionAttribute : AuthorizeAttribute, IAuthorizationRequirementData
    {
        public PermissionTargetKind Kind { get; }
        public int Value { get; }
        public PermissionAction Action { get; }

        public RequirePermissionAttribute(EntityType entity, PermissionAction action)
        {
            Kind = PermissionTargetKind.Entity;
            Value = (int)entity;
            Action = action;
        }

        public RequirePermissionAttribute(AppScreen screen)
        {
            Kind = PermissionTargetKind.Screen;
            Value = (int)screen;
            Action = PermissionAction.Read;
        }

        public IEnumerable<IAuthorizationRequirement> GetRequirements()
        {
            yield return new PermissionRequirement(Kind, Value, Action);
        }
    }

    // Checked against the database on every request. Loads the user's effective permissions once,
    // then evaluates every pending PermissionRequirement of the request.
    public sealed class PermissionHandler(UserManager<User> userManager, MarilogContext db) : IAuthorizationHandler
    {
        public async Task HandleAsync(AuthorizationHandlerContext context)
        {
            var requirements = context.PendingRequirements.OfType<PermissionRequirement>().ToList();
            if (requirements.Count == 0) return;
            if (context.User.Identity?.IsAuthenticated != true) return;

            var user = await userManager.GetUserAsync(context.User);
            if (user is null || !user.IsActive) return;

            if (user.IsSuperAdmin)
            {
                foreach (var requirement in requirements)
                    context.Succeed(requirement);
                return;
            }

            var ct = (context.Resource as HttpContext)?.RequestAborted ?? default;
            var granted = await LoadGrantedAsync(user.Id, ct);

            foreach (var requirement in requirements)
            {
                if (IsGranted(requirement, granted))
                    context.Succeed(requirement);
            }
        }

        private async Task<Dictionary<(PermissionTargetKind Kind, int Value), PermissionAction>> LoadGrantedAsync(
            Guid userId, CancellationToken ct)
        {
            var rows = await db.Set<UserRole>()
                .AsNoTracking()
                .Where(ur => ur.UserId == userId && ur.Role.IsActive)
                .SelectMany(ur => ur.Role.Permissions)
                .Select(p => new { p.TargetKind, p.TargetValue, p.Actions })
                .ToListAsync(ct);

            // Effective permissions are the union of all the user's active roles.
            var granted = new Dictionary<(PermissionTargetKind Kind, int Value), PermissionAction>();
            foreach (var row in rows)
            {
                var key = (row.TargetKind, row.TargetValue);
                granted[key] = granted.TryGetValue(key, out var existing) ? existing | row.Actions : row.Actions;
            }

            return granted;
        }

        private static bool IsGranted(
            PermissionRequirement requirement,
            Dictionary<(PermissionTargetKind Kind, int Value), PermissionAction> granted)
        {
            // Read on an open-read entity: every active, authenticated user.
            if (requirement.Kind == PermissionTargetKind.Entity
                && requirement.Action == PermissionAction.Read
                && PermissionCatalog.OpenReadEntities.Contains((EntityType)requirement.Value))
                return true;

            // Actions the catalog doesn't offer for this target (e.g. super-admin-only entities) are never granted.
            var allowed = PermissionCatalog.AllowedActions(requirement.Kind, requirement.Value);
            if ((allowed & requirement.Action) != requirement.Action)
                return false;

            return granted.TryGetValue((requirement.Kind, requirement.Value), out var actions)
                && (actions & requirement.Action) == requirement.Action;
        }
    }
}