using Marilog.Client.ErrorUniform;
using Marilog.Client.Extensions;
using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Frontend.AppTheme;
using Marilog.Contracts.Interfaces.FrontendServices;
using System;
using System.Collections.Generic;
using System.Net;
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

        public Task<AppThemeResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        => _http.GetApiAsync<AppThemeResponse>($"{Base}/{id}", ct);



        public Task<AppThemeResponse?> GetDefaultThemeAsync(CancellationToken ct = default)
        => _http.GetApiAsync<AppThemeResponse>($"{Base}/default", ct);

        public Task<IReadOnlyList<AppThemeResponse>> GetAllAsync(CancellationToken ct = default)
        => _http.GetApiListAsync<AppThemeResponse>(Base, ct);


        public Task<IReadOnlyList<AppThemeResponse>> GetActiveAsync(CancellationToken ct = default)
        => _http.GetApiListAsync<AppThemeResponse>($"{Base}/active", ct);


        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<AppThemeResponse> CreateAsync(CreateAppThemeRequest request, CancellationToken ct = default)
        {
            var response = await _http.PostApiAsync<AppThemeResponse>(Base, request, ct);
            if (response != null) 
                return response;

            return new AppThemeResponse();
        } 

        public Task UpdateAsync(int id, UpdateAppThemeRequest request, CancellationToken ct = default)
            => _http.PutApiAsync($"{Base}/{id}", request, ct);

        public Task SetAsDefaultAsync(int id, CancellationToken ct = default)
            => _http.PatchApiAsync($"{Base}/{id}/set-default", ct);

        public Task ActivateAsync(int id, CancellationToken ct = default)
            => _http.PatchApiAsync($"{Base}/{id}/activate", ct);

        public Task DeactivateAsync(int id, CancellationToken ct = default)
            => _http.PatchApiAsync($"{Base}/{id}/deactivate", ct);

        public Task DeleteAsync(int id, CancellationToken ct = default)
            => _http.DeleteApiAsync($"{Base}/{id}", ct);
    }
}
