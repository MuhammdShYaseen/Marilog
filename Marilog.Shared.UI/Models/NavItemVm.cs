

namespace Marilog.Shared.UI.Models
{
    public class NavItemVm
    {
        public string Title { get; set; } = null!;
        public string? Route { get; set; }
        public string? Icon { get; set; }
        public List<NavItemVm> Children { get; set; } = new();
    }
}
