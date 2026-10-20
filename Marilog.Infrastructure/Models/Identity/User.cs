using Microsoft.AspNetCore.Identity;

namespace Marilog.Infrastructure.Models.Identity
{
    public class User : IdentityUser<Guid>
    {
        public string FullName { get; private set; } = "";
        public bool IsActive { get; private set; }
        public bool IsSuperAdmin { get; private set; }

        private User() { }   // لـ EF

        public static User Create(string userName, string fullName, string? email = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(userName);
            ArgumentException.ThrowIfNullOrWhiteSpace(fullName);

            return new User
            {
                Id = Guid.NewGuid(),
                UserName = userName.Trim(),
                Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim(),
                FullName = fullName.Trim(),
                IsActive = true
            };
        }

        public void Rename(string fullName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
            FullName = fullName.Trim();
        }

        public void Activate() => IsActive = true;
        public void Deactivate() => IsActive = false;

        public void GrantSuperAdmin() => IsSuperAdmin = true;
        public void RevokeSuperAdmin() => IsSuperAdmin = false;
    }
}
