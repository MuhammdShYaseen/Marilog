namespace Marilog.Contracts.DTOs.Responses
{
    public sealed record RoleResponse(int Id, string Name, string? Description, bool IsActive, int UserCount);
}