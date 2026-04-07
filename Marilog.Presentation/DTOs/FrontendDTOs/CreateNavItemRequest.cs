namespace Marilog.Presentation.DTOs.FrontendDTOs
{
    public record CreateNavItemRequest(string title, string? route, string? icon, int? parentId, int order);
    
   
}
