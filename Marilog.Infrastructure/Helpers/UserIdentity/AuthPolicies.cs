using Marilog.Infrastructure.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Marilog.Infrastructure.Helpers.UserIdentity
{
    public static class AuthPolicies
    {
        public const string SuperAdmin = "SuperAdmin";
    }

    public sealed class SuperAdminRequirement : IAuthorizationRequirement { }

    public sealed class SuperAdminHandler(UserManager<User> userManager)
        : AuthorizationHandler<SuperAdminRequirement>
    {
        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context, SuperAdminRequirement requirement)
        {
            if (context.User.Identity?.IsAuthenticated != true)
                return;

            var user = await userManager.GetUserAsync(context.User);
            if (user is { IsActive: true, IsSuperAdmin: true })
                context.Succeed(requirement);
        }
    }
}