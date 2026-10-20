

namespace Marilog.Application.Interfaces.UserIdentity
{
    public interface ICurrentUserService
    {
        bool IsAuthenticated { get; }
        Guid? UserId { get; }
        string? UserName { get; }
    }
}
