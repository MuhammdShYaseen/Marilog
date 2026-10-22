using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Marilog.Contracts.DTOs.Requests.UserIdenty;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.UserIdenty;
using Marilog.Infrastructure.Models.Identity;
using Marilog.Kernel.Enums;

namespace Marilog.Infrastructure.Services.UserIdenty
{
    public sealed class UserManagementService(UserManager<User> userManager, IHttpContextAccessor httpContextAccessor) : IUserManagementService
    {
        public async Task<IReadOnlyList<UserResponse>> GetAllAsync(CancellationToken ct = default)
        {
            var now = DateTimeOffset.UtcNow;

            return await userManager.Users
                .AsNoTracking()
                .OrderBy(u => u.UserName)
                .Select(u => new UserResponse(
                    u.Id,
                    u.UserName!,
                    u.FullName,
                    u.Email,
                    u.IsActive,
                    u.IsSuperAdmin,
                    u.LockoutEnd != null && u.LockoutEnd > now))
                .ToListAsync(ct);
        }

        public async Task<UserManagementResult> CreateAsync(CreateUserRequest request, CancellationToken ct = default)
        {
            var user = User.Create(request.UserName, request.FullName, request.Email);
            var result = await userManager.CreateAsync(user, request.Password);
            return ToResult(result);
        }

        public async Task<UserManagementResult> ActivateAsync(Guid userId, CancellationToken ct = default)
        {
            var user = await FindAsync(userId);
            if (user is null) return UserManagementResult.NotFound;

            user.Activate();
            return ToResult(await userManager.UpdateAsync(user));
        }

        public async Task<UserManagementResult> DeactivateAsync(Guid userId, CancellationToken ct = default)
        {
            var user = await FindAsync(userId);
            if (user is null) return UserManagementResult.NotFound;
            if (IsSelf(user)) return UserManagementResult.CannotModifySelf;
            if (await IsLastActiveSuperAdminAsync(user, ct)) return UserManagementResult.LastSuperAdmin;

            user.Deactivate();
            // بيحفظ التعطيل وبيغيّر الـ stamp، فالجلسة المفتوحة بتنلغى خلال دقيقة
            return ToResult(await userManager.UpdateSecurityStampAsync(user));
        }

        public async Task<UserManagementResult> GrantSuperAdminAsync(Guid userId, CancellationToken ct = default)
        {
            var user = await FindAsync(userId);
            if (user is null) return UserManagementResult.NotFound;

            user.GrantSuperAdmin();
            return ToResult(await userManager.UpdateAsync(user));
        }

        public async Task<UserManagementResult> RevokeSuperAdminAsync(Guid userId, CancellationToken ct = default)
        {
            var user = await FindAsync(userId);
            if (user is null) return UserManagementResult.NotFound;
            if (IsSelf(user)) return UserManagementResult.CannotModifySelf;
            if (await IsLastActiveSuperAdminAsync(user, ct)) return UserManagementResult.LastSuperAdmin;

            user.RevokeSuperAdmin();
            return ToResult(await userManager.UpdateAsync(user));
        }

        public async Task<UserManagementResult> ResetPasswordAsync(Guid userId, ResetPasswordRequest request, CancellationToken ct = default)
        {
            var user = await FindAsync(userId);
            if (user is null) return UserManagementResult.NotFound;

            // نتأكد من كلمة السر الجديدة قبل ما نشيل القديمة
            foreach (var validator in userManager.PasswordValidators)
            {
                var check = await validator.ValidateAsync(userManager, user, request.NewPassword);
                if (!check.Succeeded) return UserManagementResult.InvalidPassword;
            }

            var removed = await userManager.RemovePasswordAsync(user);
            if (!removed.Succeeded) return ToResult(removed);

            var added = await userManager.AddPasswordAsync(user, request.NewPassword);
            if (!added.Succeeded) return ToResult(added);

            // فك القفل إذا المستخدم كان مقفول من كثرة المحاولات
            await userManager.SetLockoutEndDateAsync(user, null);
            await userManager.ResetAccessFailedCountAsync(user);

            return UserManagementResult.Success;
        }

        private Task<User?> FindAsync(Guid userId) =>
            userManager.FindByIdAsync(userId.ToString());

        private bool IsSelf(User user)
        {
            var principal = httpContextAccessor.HttpContext?.User;
            return principal is not null && userManager.GetUserId(principal) == user.Id.ToString();
        }

        private async Task<bool> IsLastActiveSuperAdminAsync(User user, CancellationToken ct)
        {
            if (!user.IsActive || !user.IsSuperAdmin) return false;

            var activeAdmins = await userManager.Users
                .CountAsync(u => u.IsActive && u.IsSuperAdmin, ct);

            return activeAdmins <= 1;
        }

        private static UserManagementResult ToResult(IdentityResult result)
        {
            if (result.Succeeded) return UserManagementResult.Success;
            if (result.Errors.Any(e => e.Code == "DuplicateUserName")) return UserManagementResult.UserNameTaken;
            if (result.Errors.Any(e => e.Code.StartsWith("Password"))) return UserManagementResult.InvalidPassword;
            return UserManagementResult.Failed;
        }
    }
}