using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.CountryDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using System.Net.Http.Json;

namespace Marilog.Client.Services.SystemServices
{
    public class CountryService : ICountryService
    {
        private readonly HttpClient _http;
        private const string Base = "api/countries";

        public CountryService(HttpClient http) => _http = http;

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<CountryResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<CountryResponse>>($"{Base}/{id}", ct);
            return response?.Data;
        }

        public async Task<CountryResponse?> GetByCodeAsync(string code, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<CountryResponse>>($"{Base}/by-code/{Uri.EscapeDataString(code)}", ct);
            return response?.Data;
        }

        public async Task<IReadOnlyList<CountryResponse>> GetAllAsync(CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<IReadOnlyList<CountryResponse>>>(Base, ct);
            return response?.Data ?? [];
        }

        public async Task<IReadOnlyList<CountryResponse>> GetActiveAsync(CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<IReadOnlyList<CountryResponse>>>($"{Base}/active", ct);
            return response?.Data ?? [];
        }

        // No backend endpoint for this — handled client-side
        public async Task<bool> ExistsByCodeAsync(string code, CancellationToken ct = default)
        {
            var country = await GetByCodeAsync(code, ct);
            return country is not null;
        }

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<CountryResponse> CreateAsync(string countryCode, string countryName, CancellationToken ct = default)
        {
            var request = new CreateCountryRequest { CountryCode = countryCode, CountryName = countryName };
            var http = await _http.PostAsJsonAsync(Base, request, ct);
            http.EnsureSuccessStatusCode();
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<CountryResponse>>(ct);
            return response!.Data!;
        }

        public async Task<IReadOnlyList<CountryResponse>> CreateRangeAsync(IEnumerable<CreateCountryRequest> commands, CancellationToken ct = default)
        {
            var http = await _http.PostAsJsonAsync($"{Base}/batch", commands, ct);
            http.EnsureSuccessStatusCode();
            var response = await http.Content.ReadFromJsonAsync<IReadOnlyList<CountryResponse>>(ct);
            return response ?? [];
        }

        public async Task UpdateAsync(int id, string countryCode, string countryName, CancellationToken ct = default)
        {
            var request = new UpdateCountryRequest { CountryCode = countryCode, CountryName = countryName };
            var http = await _http.PutAsJsonAsync($"{Base}/{id}", request, ct);
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