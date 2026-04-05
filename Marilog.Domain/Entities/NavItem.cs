

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

        private readonly List<NavItem> _children = new();
        public IReadOnlyCollection<NavItem> Children => _children.AsReadOnly();

        private NavItem() { }

        private NavItem(string title, string? route, string? icon, int? parentId, int order)
        {
            Title = title;
            Route = route;
            Icon = icon;
            ParentId = parentId;
            Order = order;
            IsActive = true;
        }

        public static NavItem Create(string title, string? route, string? icon, int? parentId, int order)
            => new(title, route, icon, parentId, order);
    }
}
