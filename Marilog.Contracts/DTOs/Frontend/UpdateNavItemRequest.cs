namespace Marilog.Contracts.DTOs.Frontend
{
    public class UpdateNavItemRequest
    {
        public string Title { get; set; } = default!;

        public string? Route { get; set; }

        public string? Icon { get; set; }

        public int? ParentId { get; set; }

        public int Order { get; set; }
    }
}
