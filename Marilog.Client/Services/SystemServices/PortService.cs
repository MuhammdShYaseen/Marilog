using Marilog.Client.Extensions;
using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.PortDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using System.Net.Http.Json;

namespace Marilog.Client.Services.SystemServices
{
    public class PortService : IPortService
    {
        private readonly HttpClient _http;
        private const string Base = "api/port";

        public PortService(HttpClient http) => _http = http;

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<PortResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<ApiResponse<PortResponse>>($"{Base}/{id}", ct);
            return response?.Data;
        }

        public async Task<PortResponse?> GetByCodeAsync(string code, CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<ApiResponse<PortResponse>>($"{Base}/code/{Uri.EscapeDataString(code)}", ct);
            return response?.Data;
        }

        public async Task<IReadOnlyList<PortResponse>> GetAllAsync(CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<ApiResponse<IReadOnlyList<PortResponse>>>(Base, ct);
            return response?.Data ?? [];
        }

        public async Task<IReadOnlyList<PortResponse>> GetActiveAsync(CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<ApiResponse<IReadOnlyList<PortResponse>>>($"{Base}/active", ct);
            return response?.Data ?? [];
        }

        public async Task<IReadOnlyList<PortResponse>> GetByCountryAsync(int countryId, CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<ApiResponse<IReadOnlyList<PortResponse>>>($"{Base}/country/{countryId}", ct);
            return response?.Data ?? [];
        }

        public async Task<bool> ExistsByCodeAsync(string code, CancellationToken ct = default)
        {
            var result = await GetByCodeAsync(code, ct);
            return result is not null;
        }

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<PortResponse> CreateAsync(string portCode, string portName, int? countryId = null, CancellationToken ct = default)
        {
            var request = new CreatePortRequest { PortCode = portCode, PortName = portName, CountryId = countryId };
            var http = await _http.PostAsJsonAsync(Base, request, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<PortResponse>>(ct);
            return response!.Data!;
        }

        public async Task<IReadOnlyList<PortResponse>> CreateRangeAsync(IEnumerable<CreatePortRequest> commands, CancellationToken ct = default)
        {
            var http = await _http.PostAsJsonAsync($"{Base}/batch", commands, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<PortResponse>>>(ct);
            return response?.Data ?? [];
        }

        public async Task UpdateAsync(int id, string portCode, string portName, int? countryId = null, CancellationToken ct = default)
        {
            var request = new UpdatePortRequest { PortCode = portCode, PortName = portName, CountryId = countryId };
            var http = await _http.PutAsJsonAsync($"{Base}/{id}", request, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
        }

        public async Task ActivateAsync(int id, CancellationToken ct = default)
        {
            var http = await _http.PostAsync($"{Base}/{id}/activate", null, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
        }

        public async Task DeactivateAsync(int id, CancellationToken ct = default)
        {
            var http = await _http.PostAsync($"{Base}/{id}/deactivate", null, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var http = await _http.DeleteAsync($"{Base}/{id}", ct);
            await http.EnsureSuccessWithMessageAsync(ct);
        }
    }
}