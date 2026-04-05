using Marilog.Application.DTOs.Responses;

namespace Marilog.Application.Interfaces.FrontendServices
{
    public interface INavigationService
    {
        Task<List<NavItemResponse>> GetAsync(CancellationToken ct = default);
    }
}
