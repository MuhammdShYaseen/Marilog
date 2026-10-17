

namespace Marilog.Contracts.DTOs.Responses
{
    public class NavItemExport
    {
        public string Title { get; set; } = null!;
        public string? Route { get; set; }
        public string? Icon { get; set; }
        public bool IsActive { get; set; } = true;
        public List<NavItemExport> Children { get; set; } = new();
    }
}
