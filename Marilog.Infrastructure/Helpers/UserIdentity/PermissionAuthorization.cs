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
    public sealed class PermissionHandler(UserManager<User> userManager, UserPermissionReader permissionReader)
        : IAuthorizationHandler
    {
        public async Task HandleAsync(AuthorizationHandlerContext context)
        {
            var requirements = context.PendingRequirements.OfType<PermissionRequirement>().ToList();
            if (requirements.Count == 0) return;
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
        }
    }
}