using Marilog.Contracts.DTOs.Requests.UserIdenty;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Kernel.Enums;

namespace Marilog.Contracts.Interfaces.Services.UserIdenty
{
    public interface IAuthService
    {
        Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken ct = default);
        Task LogoutAsync(CancellationToken ct = default);
        Task<CurrentUserResponse?> GetCurrentUserAsync(CancellationToken ct = default);
    }
}
