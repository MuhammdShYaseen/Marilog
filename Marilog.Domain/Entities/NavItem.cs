

using Marilog.Domain.Common;

namespace Marilog.Domain.Entities
{
    public class NavItem : Entity
    {
        public string Title { get; private set; } = null!;
        public string? Route { get; private set; }
        public string? Icon { get; private set; }
        public int? ParentId { get; private set; }
        public int Order { get; private set; }

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
    }
}
