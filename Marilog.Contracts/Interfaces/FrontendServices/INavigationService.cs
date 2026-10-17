using Marilog.Contracts.DTOs.Frontend.NavItem;
using Marilog.Contracts.DTOs.Responses;

namespace Marilog.Contracts.Interfaces.FrontendServices
{
    public interface INavigationService
    {
        Task<List<NavItemResponse>> GetAsync(CancellationToken ct = default);
        Task<NavItemResponse> CreateAsync (string title, string? route, string? icon, int? parentId, int order, CancellationToken ct = default);
        Task<List<NavItemResponse>> CreateRangeAsync(List<CreateNavItemRequest> items, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);
        Task<NavItemResponse> UpdateAsync(int id, string title, string? route, string? icon, int? parentId, int order, CancellationToken ct = default);


        Task<List<NavItemResponse>> GetAllAsync(CancellationToken ct = default);
        Task SaveLayoutAsync(List<NavItemLayoutRequest> items, CancellationToken ct = default);
        Task SetActiveAsync(int id, bool isActive, CancellationToken ct = default);
        Task<List<NavItemExport>> ExportAsync(CancellationToken ct = default);
        Task ImportAsync(List<NavItemExport> items, CancellationToken ct = default);


    }
}
