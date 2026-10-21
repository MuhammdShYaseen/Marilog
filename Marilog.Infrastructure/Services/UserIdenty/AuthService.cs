
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Marilog.Contracts.DTOs.Requests.UserIdenty;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Infrastructure.Models.Identity;
using Marilog.Kernel.Enums;
using Marilog.Contracts.Interfaces.Services.UserIdenty;

namespace Marilog.Infrastructure.Services.UserIdenty
{
    public sealed class AuthService(SignInManager<User> signInManager, UserManager<User> userManager, IHttpContextAccessor httpContextAccessor) : IAuthService
    {
        public async Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken ct = default)
        {
            var user = await userManager.FindByNameAsync(request.UserName.Trim());
            if (user is null || !user.IsActive)
                return LoginResult.InvalidCredentials;

            var result = await signInManager.PasswordSignInAsync(
                user, request.Password, isPersistent: false, lockoutOnFailure: true);

            if (result.Succeeded) return LoginResult.Success;
            if (result.IsLockedOut) return LoginResult.LockedOut;
            return LoginResult.InvalidCredentials;
        }

        public Task LogoutAsync(CancellationToken ct = default) => signInManager.SignOutAsync();

        public async Task<CurrentUserResponse?> GetCurrentUserAsync(CancellationToken ct = default)
        {
            var principal = httpContextAccessor.HttpContext?.User;
            if (principal?.Identity?.IsAuthenticated != true)
                return null;

            var user = await userManager.GetUserAsync(principal);
            if (user is null || !user.IsActive)
                return null;

            return new CurrentUserResponse(user.Id, user.UserName!, user.FullName, user.IsSuperAdmin);
        }
    }
}
