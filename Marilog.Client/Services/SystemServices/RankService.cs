using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.RankDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Kernel.Enums;
using System.Net.Http.Json;

namespace Marilog.Client.Services.SystemServices
{
    public class RankService : IRankService
    {
        private readonly HttpClient _http;
        private const string Base = "api/rank";

        public RankService(HttpClient http) => _http = http;

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<RankResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<RankResponse>>($"{Base}/{id}", ct);
            return response?.Data;
        }

        public async Task<RankResponse?> GetByCodeAsync(string code, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<RankResponse>>($"{Base}/code/{Uri.EscapeDataString(code)}", ct);
            return response?.Data;
        }

        public async Task<IReadOnlyList<RankResponse>> GetAllAsync(CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<IReadOnlyList<RankResponse>>>(Base, ct);
            return response?.Data ?? [];
        }

        public async Task<IReadOnlyList<RankResponse>> GetActiveAsync(CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<IReadOnlyList<RankResponse>>>($"{Base}/active", ct);
            return response?.Data ?? [];
        }

        public async Task<IReadOnlyList<RankResponse>> GetByDepartmentAsync(Department department, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<IReadOnlyList<RankResponse>>>($"{Base}/department/{department}", ct);
            return response?.Data ?? [];
        }

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<RankResponse> CreateAsync(string rankCode, string rankName, Department department, CancellationToken ct = default)
        {
            var request = new CreateRankRequest { RankCode = rankCode, RankName = rankName, Department = department };
            var http = await _http.PostAsJsonAsync(Base, request, ct);
            http.EnsureSuccessStatusCode();
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<RankResponse>>(ct);
            return response!.Data!;
        }

        public async Task<IReadOnlyList<RankResponse>> CreateRangeAsync(IEnumerable<CreateRankRequest> commands, CancellationToken ct = default)
        {
            var http = await _http.PostAsJsonAsync($"{Base}/batch", commands, ct);
            http.EnsureSuccessStatusCode();
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<RankResponse>>>(ct);
            return response?.Data ?? [];
        }

        public async Task UpdateAsync(int id, string rankCode, string rankName, Department department, CancellationToken ct = default)
        {
            var request = new UpdateRankRequest { RankCode = rankCode, RankName = rankName, Department = department };
            var http = await _http.PutAsJsonAsync($"{Base}/{id}", request, ct);
            http.EnsureSuccessStatusCode();
        }

        public async Task ActivateAsync(int id, CancellationToken ct = default)
        {
            var http = await _http.PostAsync($"{Base}/{id}/activate", null, ct);
            http.EnsureSuccessStatusCode();
        }

        public async Task DeactivateAsync(int id, CancellationToken ct = default)
        {
            var http = await _http.PostAsync($"{Base}/{id}/deactivate", null, ct);
            http.EnsureSuccessStatusCode();
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var http = await _http.DeleteAsync($"{Base}/{id}", ct);
            http.EnsureSuccessStatusCode();
        }
    }
}