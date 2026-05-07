namespace Marilog.Contracts.DTOs.Frontend.NavItem
{
    public class CreateNavItemRequest
    {
        public string Title { get;  init; } = null!;
        public int? ParentId { get; init; }
        public string? Route { get;  init; }
        public string? Icon { get;  init; }
        public int Order { get;  init; }
    }
}
