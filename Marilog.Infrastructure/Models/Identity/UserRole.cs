using Marilog.Domain.Common;

namespace Marilog.Infrastructure.Models.Identity
{
    public class UserRole : Entity
    {
        public Guid UserId { get; private set; }

        public int RoleId { get; private set; }
        public Role Role { get; private set; } = null!;

        private UserRole() { }

        public static UserRole Create(Guid userId, int roleId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("UserId is required.", nameof(userId));
            if (roleId <= 0)
                throw new ArgumentException("RoleId is required.", nameof(roleId));

            return new UserRole
            {
                UserId = userId,
                RoleId = roleId
            };
        }
    }
}