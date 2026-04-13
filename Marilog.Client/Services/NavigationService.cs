using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Frontend;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.FrontendServices;
using System.Net.Http.Json;

namespace Marilog.Client.Services
{
    internal class NavigationService : INavigationService
    {
        private readonly HttpClient _http;
        private static List<NavItemResponse>? _cachedItems;
        public NavigationService(HttpClient http)
        {
            _http = http;
        }
        public Task<NavItemResponse> CreateAsync(string title, string? route, string? icon, int? parentId, int order, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<List<NavItemResponse>> CreateRangeAsync(List<CreateNavItemRequest> items, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(int id, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public async Task<List<NavItemResponse>> GetAsync(CancellationToken ct = default)
        {
            if (_cachedItems != null)
                return _cachedItems;

            var res = await _http.GetFromJsonAsync<ApiResponse<List<NavItemResponse>>>("api/navigation", ct);
            _cachedItems = res?.Data ?? new();

            return _cachedItems;
        }

        public Task<NavItemResponse> UpdateAsync(int id, string title, string? route, string? icon, int? parentId, int order, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
