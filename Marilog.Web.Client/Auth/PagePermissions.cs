using System.Globalization;
using Marilog.Kernel.Enumerations;
using Marilog.Kernel.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Marilog.Web.Client.Auth
{
    // Usage on a host page: @attribute [RequirePage(EntityType.Vessel)] or [RequirePage(AppScreen.FinancialDashboard)].
    // The page opens only if the user has Read on the target. Cosmetic: the server is the real gate.
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class RequirePageAttribute : AuthorizeAttribute
    {
        public RequirePageAttribute(EntityType entity)
            : base(PagePermissionPolicy.For(PermissionTargetKind.Entity, (int)entity)) { }

        public RequirePageAttribute(AppScreen screen)
            : base(PagePermissionPolicy.For(PermissionTargetKind.Screen, (int)screen)) { }
    }

    // Policy names look like "perm:1:7" (kind:value).
    public static class PagePermissionPolicy
    {
        private const string Prefix = "perm:";

        public static string For(PermissionTargetKind kind, int value)
            => string.Create(CultureInfo.InvariantCulture, $"{Prefix}{(int)kind}:{value}");

        public static bool TryParse(string policyName, out PermissionTargetKind kind, out int value)
        {
            kind = default;
            value = default;

            if (!policyName.StartsWith(Prefix, StringComparison.Ordinal)) return false;

            var parts = policyName[Prefix.Length..].Split(':');
            if (parts.Length != 2
                || !int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var k)
                || !int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
                return false;

            kind = (PermissionTargetKind)k;
            return true;
        }
    }

    public sealed record PagePermissionRequirement(PermissionTargetKind Kind, int Value) : IAuthorizationRequirement;

    // Same rule as the server, read from the permission claims built by CookieAuthStateProvider.
    public sealed class PagePermissionHandler : AuthorizationHandler<PagePermissionRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, PagePermissionRequirement requirement)
        {
            if (context.User.GetPermissions().Can(requirement.Kind, requirement.Value, PermissionAction.Read))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }

    // Builds "perm:*" policies on demand; every other policy name goes to the default provider.
    public sealed class PagePermissionPolicyProvider(IOptions<AuthorizationOptions> options) : IAuthorizationPolicyProvider
    {
        private readonly DefaultAuthorizationPolicyProvider _fallback = new(options);

        public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            if (!PagePermissionPolicy.TryParse(policyName, out var kind, out var value))
                return _fallback.GetPolicyAsync(policyName);

            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new PagePermissionRequirement(kind, value))
                .Build();

            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallback.GetDefaultPolicyAsync();

        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _fallback.GetFallbackPolicyAsync();
    }
}