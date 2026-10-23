using Marilog.Domain.Common;
using Marilog.Kernel.Enums;

namespace Marilog.Domain.Entities.Frontend
{
    public class NavItem : Entity
    {
        public string Title { get; private set; } = null!;
        public string? Route { get; private set; }
        public string? Icon { get; private set; }
        public int? ParentId { get; private set; }
        public int Order { get; private set; }
        public PermissionTargetKind? RequiredTargetKind { get; private set; }
        public int? RequiredTargetValue { get; private set; }

        public List<NavItem> Children { get; private set; } = new(); // EF Core likes this

        private NavItem() { }

        public static NavItem Create(string title, string? route, string? icon, int? parentId, int order)
        => new NavItem
        {
            Title = title,
            Route = route,
            Icon = icon,
            ParentId = parentId,
            Order = order,
            IsActive = true
        };

        public void Update(string title, string? route, string? icon, int? parentId, int order)
        {
            Title = title;
            Route = route;
            Icon = icon;
            ParentId = parentId;
            Order = order;
        }

        public void Move(int? parentId, int order)
        {
            ParentId = parentId;
            Order = order;
        }

        public void SetActive(bool isActive) => IsActive = isActive;

        public void AddChild(NavItem child) => Children.Add(child);
        // No required permission means the link is visible to all. Read is implied.
        public void SetRequiredPermission(PermissionTargetKind? kind, int? value)
        {
            if (kind.HasValue != value.HasValue)
                throw new ArgumentException("Target kind and value must both be set or both be null.");

            RequiredTargetKind = kind;
            RequiredTargetValue = value;
            Touch();
        }
    }
}
