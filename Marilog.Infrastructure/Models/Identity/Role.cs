using Marilog.Domain.Common;

namespace Marilog.Infrastructure.Models.Identity
{
    public class Role : Entity
    {
        public string Name { get; private set; } = null!;
        public string? Description { get; private set; }

        public List<RolePermission> Permissions { get; private set; } = new();

        private Role() { }

        public static Role Create(string name, string? description = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            return new Role
            {
                Name = name.Trim(),
                Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim()
            };
        }

        public void Update(string name, string? description)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            Name = name.Trim();
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
            Touch();
        }
    }
}