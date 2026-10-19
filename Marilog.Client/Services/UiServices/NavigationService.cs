using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Frontend.NavItem;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.FrontendServices;
using System.Net.Http.Json;

namespace Marilog.Client.Services.UiServices
{
    internal class NavigationService : INavigationService
    {
        private readonly HttpClient _http;
        private const string Base = "api/navigation";
        private static List<NavItemResponse>? _cachedItems;

        public NavigationService(HttpClient http)
        {
            _http = http;
        }

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<List<NavItemResponse>> GetAsync(CancellationToken ct = default)
        {
            if (_cachedItems != null)
                return _cachedItems;

            var res = await _http.GetCheckedAsync<ApiResponse<List<NavItemResponse>>>(Base, ct);
            _cachedItems = res?.Data ?? new();

            return _cachedItems;
        }

        public async Task<List<NavItemResponse>> GetAllAsync(CancellationToken ct = default)
        {
            var res = await _http.GetCheckedAsync<ApiResponse<List<NavItemResponse>>>($"{Base}/all", ct);
            return res?.Data ?? new();
        }

        public async Task<List<NavItemExport>> ExportAsync(CancellationToken ct = default)
        {
            return await _http.GetCheckedAsync<List<NavItemExport>>($"{Base}/export", ct) ?? new();
        }

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<NavItemResponse> CreateAsync(string title, string? route, string? icon, int? parentId, int order, CancellationToken ct = default)
        {
            var request = new CreateNavItemRequest { Title = title, Route = route, Icon = icon, ParentId = parentId, Order = order };
            var http = await _http.PostAsJsonAsync(Base, request, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
            _cachedItems = null;

            var response = await http.Content.ReadFromJsonAsync<ApiResponse<NavItemResponse>>(ct);
            return response!.Data!;
        }

        public async Task<List<NavItemResponse>> CreateRangeAsync(List<CreateNavItemRequest> items, CancellationToken ct = default)
        {
            var http = await _http.PostAsJsonAsync($"{Base}/batch", items, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
            _cachedItems = null;

            var response = await http.Content.ReadFromJsonAsync<ApiResponse<List<NavItemResponse>>>(ct);
            return response?.Data ?? new();
        }

        public async Task<NavItemResponse> UpdateAsync(int id, string title, string? route, string? icon, int? parentId, int order, CancellationToken ct = default)
        {
            var request = new UpdateNavItemRequest { Title = title, Route = route, Icon = icon, ParentId = parentId, Order = order };
            var http = await _http.PutAsJsonAsync($"{Base}/{id}", request, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
            _cachedItems = null;

            var response = await http.Content.ReadFromJsonAsync<ApiResponse<NavItemResponse>>(ct);
            return response!.Data!;
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var http = await _http.DeleteAsync($"{Base}/{id}", ct);
            await http.EnsureSuccessWithMessageAsync(ct);
            _cachedItems = null;
        }

        public async Task SetActiveAsync(int id, bool isActive, CancellationToken ct = default)
        {
            var action = isActive ? "activate" : "deactivate";
            var http = await _http.PostAsync($"{Base}/{id}/{action}", null, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
            _cachedItems = null;
        }

        public async Task SaveLayoutAsync(List<NavItemLayoutRequest> items, CancellationToken ct = default)
        {
            var http = await _http.PutAsJsonAsync($"{Base}/layout", items, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
            _cachedItems = null;
        }

        public async Task ImportAsync(List<NavItemExport> items, CancellationToken ct = default)
        {
            var http = await _http.PostAsJsonAsync($"{Base}/import", items, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
            _cachedItems = null;
        }
    }
}