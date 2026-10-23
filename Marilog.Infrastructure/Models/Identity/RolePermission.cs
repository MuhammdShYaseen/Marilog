using Marilog.Domain.Common;
using Marilog.Kernel.Enums;

namespace Marilog.Infrastructure.Models.Identity
{
    public class RolePermission : Entity
    {
        public int RoleId { get; private set; }
        public Role Role { get; private set; } = null!;

        public PermissionTargetKind TargetKind { get; private set; }
        public int TargetValue { get; private set; }
        public PermissionAction Actions { get; private set; }

        private RolePermission() { }

        public static RolePermission Create(PermissionTargetKind targetKind, int targetValue, PermissionAction actions)
        {
            if (actions == PermissionAction.None)
                throw new ArgumentException("A permission must grant at least one action.", nameof(actions));

            return new RolePermission
            {
                TargetKind = targetKind,
                TargetValue = targetValue,
                Actions = actions
            };
        }

        public void SetActions(PermissionAction actions)
        {
            if (actions == PermissionAction.None)
                throw new ArgumentException("A permission must grant at least one action.", nameof(actions));

            Actions = actions;
            Touch();
        }
    }
}