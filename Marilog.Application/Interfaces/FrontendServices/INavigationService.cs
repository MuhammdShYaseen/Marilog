using Marilog.Application.DTOs.Commands.Frontend;
using Marilog.Application.DTOs.Responses;

namespace Marilog.Application.Interfaces.FrontendServices
{
    public interface INavigationService
    {
        Task<List<NavItemResponse>> GetAsync(CancellationToken ct = default);
        Task<NavItemResponse> CreateAsync (string title, string? route, string? icon, int? parentId, int order, CancellationToken ct = default);
        Task<List<NavItemResponse>> CreateRangeAsync(List<CreateNavItemRequest> items, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);
        Task<NavItemResponse> UpdateAsync(int id, string title, string? route, string? icon, int? parentId, int order, CancellationToken ct = default);


    }
}
