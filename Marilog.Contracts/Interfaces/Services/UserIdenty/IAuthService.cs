using Marilog.Contracts.DTOs.Requests.UserIdenty;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Kernel.Enums;

namespace Marilog.Contracts.Interfaces.Services.UserIdenty
{
    public interface IAuthService
    {
        Task<LoginResult> LoginAsync(LoginRequest request);
        Task LogoutAsync();
        Task<CurrentUserResponse?> GetCurrentUserAsync();
    }
}
