

namespace Marilog.Contracts.DTOs.Frontend.NavItem
{
    public class NavItemLayoutRequest
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public int Order { get; set; }
    }
}
