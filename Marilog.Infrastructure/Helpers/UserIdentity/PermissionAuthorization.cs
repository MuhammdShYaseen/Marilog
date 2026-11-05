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
    // Checked against the database on every request. Loads the user's effective permissions once,
    // then evaluates every pending PermissionRequirement with the shared rule in EffectivePermissions.
    // Checked against the database on every request. Loads the user's effective permissions once,
    // then evaluates every pending PermissionRequirement / AnyPermissionRequirement with the shared rule in EffectivePermissions.
    public sealed class PermissionHandler(UserManager<User> userManager, UserPermissionReader permissionReader) : IAuthorizationHandler
    {
        public async Task HandleAsync(AuthorizationHandlerContext context)
        {
            var requirements = context.PendingRequirements.OfType<PermissionRequirement>().ToList();
            var anyRequirements = context.PendingRequirements.OfType<AnyPermissionRequirement>().ToList();

            if (requirements.Count == 0 && anyRequirements.Count == 0) return;
            if (context.User.Identity?.IsAuthenticated != true) return;

            var user = await userManager.GetUserAsync(context.User);
            if (user is null || !user.IsActive) return;

            var ct = (context.Resource as HttpContext)?.RequestAborted ?? default;
            var permissions = await permissionReader.GetAsync(user, ct);

            foreach (var requirement in requirements)
            {
                if (permissions.Can(requirement.Kind, requirement.Value, requirement.Action))
                    context.Succeed(requirement);
            }

            foreach (var requirement in anyRequirements)
            {
                var granted = Enum.GetValues<EntityType>()
                    .Where(e => e != EntityType.NONE)
                    .Any(e => requirement.Actions.Any(a => permissions.Can(PermissionTargetKind.Entity, (int)e, a)));

                if (granted)
                    context.Succeed(requirement);
            }
        }
    }

    public sealed record AnyPermissionRequirement(PermissionAction[] Actions) : IAuthorizationRequirement;

    // Usage: [RequireAnyPermission(PermissionAction.Create, PermissionAction.Update)]
    // Passes when the user holds any of the given actions on any entity.
    // For entry points that can turn into any record (Inbox, document extraction).
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class RequireAnyPermissionAttribute : AuthorizeAttribute, IAuthorizationRequirementData
    {
        public PermissionAction[] Actions { get; }

        public RequireAnyPermissionAttribute(params PermissionAction[] actions) => Actions = actions;

        public IEnumerable<IAuthorizationRequirement> GetRequirements()
        {
            yield return new AnyPermissionRequirement(Actions);
        }
    }
}