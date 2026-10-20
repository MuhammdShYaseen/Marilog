
namespace Marilog.Contracts.DTOs.Responses
{
    public record CurrentUserResponse(Guid Id, string UserName, string FullName, bool IsSuperAdmin);
}
