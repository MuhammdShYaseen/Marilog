using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.OfficeDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using System.Net.Http.Json;

namespace Marilog.Client.Services.SystemServices
{
    public class OfficeService : IOfficeService
    {
        private readonly HttpClient _http;
        private const string Base = "api/offices";

        public OfficeService(HttpClient http) => _http = http;

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<OfficeResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<OfficeResponse>>($"{Base}/{id}", ct);
            return response?.Data;
        }

        public async Task<IReadOnlyList<OfficeResponse>> GetAllAsync(CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<IReadOnlyList<OfficeResponse>>>(Base, ct);
            return response?.Data ?? [];
        }

        public async Task<IReadOnlyList<OfficeResponse>> GetActiveAsync(CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<IReadOnlyList<OfficeResponse>>>($"{Base}/active", ct);
            return response?.Data ?? [];
        }

        public async Task<IReadOnlyList<OfficeResponse>> GetByCountryAsync(int countryId, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<IReadOnlyList<OfficeResponse>>>($"{Base}/by-country/{countryId}", ct);
            return response?.Data ?? [];
        }

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<OfficeResponse> CreateAsync(string officeName, string city, int countryId,
            string? address = null, string? phone = null, string? contactName = null,
            CancellationToken ct = default)
        {
            var request = new CreateOfficeRequest
            {
                OfficeName = officeName,
                City = city,
                CountryId = countryId,
                Address = address,
                Phone = phone,
                ContactName = contactName
            };

            var http = await _http.PostAsJsonAsync(Base, request, ct);
            http.EnsureSuccessStatusCode();
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<OfficeResponse>>(ct);
            return response!.Data!;
        }

        public async Task<IReadOnlyList<OfficeResponse>> CreateRangeAsync(IEnumerable<CreateOfficeRequest> commands, CancellationToken ct = default)
        {
            var http = await _http.PostAsJsonAsync($"{Base}/batch", commands, ct);
            http.EnsureSuccessStatusCode();
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<OfficeResponse>>>(ct);
            return response?.Data ?? [];
        }

        public async Task UpdateAsync(int id, string officeName, string city, int countryId,
            string? address = null, string? phone = null, string? contactName = null,
            CancellationToken ct = default)
        {
            var request = new UpdateOfficeRequest
            {
                OfficeName = officeName,
                City = city,
                CountryId = countryId,
                Address = address,
                Phone = phone,
                ContactName = contactName
            };

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