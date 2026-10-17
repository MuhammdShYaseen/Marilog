namespace Marilog.Contracts.DTOs.Responses
{
    public class NavItemResponse
    {
        public string Title { get; set; } = null!;
        public string? Route { get; set; }
        public string? Icon { get; set; }
        public int Order { get; set; }


        public int Id { get; set; }
        public int? ParentId { get; set; }
        public bool IsActive { get; set; }

        public List<NavItemResponse> Children { get; set; } = new();
    }
}
