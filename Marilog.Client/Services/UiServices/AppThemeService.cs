using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Frontend.AppTheme;
using Marilog.Contracts.Interfaces.FrontendServices;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace Marilog.Client.Services.UiServices
{
    public class AppThemeService : IAppThemeService
    {
        private readonly HttpClient _http;
        private const string Base = "api/app-themes";

        public AppThemeService(HttpClient http) => _http = http;

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<AppThemeResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<AppThemeResponse>>($"{Base}/{id}", ct);
            return response?.Data;
        }

        public async Task<AppThemeResponse?> GetDefaultThemeAsync(CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<AppThemeResponse>>($"{Base}/default", ct);
            return response?.Data;
        }

        public async Task<IReadOnlyList<AppThemeResponse>> GetAllAsync(CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<IReadOnlyList<AppThemeResponse>>>(Base, ct);
            return response?.Data ?? [];
        }

        public async Task<IReadOnlyList<AppThemeResponse>> GetActiveAsync(CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<IReadOnlyList<AppThemeResponse>>>($"{Base}/active", ct);
            return response?.Data ?? [];
        }

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<AppThemeResponse> CreateAsync(CreateAppThemeRequest request, CancellationToken ct = default)
        {
            var http = await _http.PostAsJsonAsync(Base, request, ct);
            http.EnsureSuccessStatusCode();
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<AppThemeResponse>>(ct);
            return response!.Data!;
        }

        public async Task UpdateAsync(int id, UpdateAppThemeRequest request, CancellationToken ct = default)
        {
            var http = await _http.PutAsJsonAsync($"{Base}/{id}", request, ct);
            http.EnsureSuccessStatusCode();
        }

        public async Task SetAsDefaultAsync(int id, CancellationToken ct = default)
        {
            var http = await _http.PatchAsync($"{Base}/{id}/set-default", null, ct);
            http.EnsureSuccessStatusCode();
        }

        public async Task ActivateAsync(int id, CancellationToken ct = default)
        {
            var http = await _http.PatchAsync($"{Base}/{id}/activate", null, ct);
            http.EnsureSuccessStatusCode();
        }

        public async Task DeactivateAsync(int id, CancellationToken ct = default)
        {
            var http = await _http.PatchAsync($"{Base}/{id}/deactivate", null, ct);
            http.EnsureSuccessStatusCode();
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var http = await _http.DeleteAsync($"{Base}/{id}", ct);
            http.EnsureSuccessStatusCode();
        }
    }
}
