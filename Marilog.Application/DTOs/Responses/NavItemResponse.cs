
namespace Marilog.Application.DTOs.Responses
{
    public class NavItemResponse
    {
        public string Title { get; set; } = null!;
        public string? Route { get; set; }
        public string? Icon { get; set; }
        public int Order { get; set; }
        public List<NavItemResponse> Children { get; set; } = new();
    }
}
