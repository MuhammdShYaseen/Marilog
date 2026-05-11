using Marilog.Contracts.DTOs.Requests.CompanyDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Common;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using System.Net.Http.Json;
using Marilog.Client.Extensions;

namespace Marilog.Client.Services.SystemServices
{
    public class CompanyService : ICompanyService
    {
        private readonly HttpClient _http;
        private const string Base = "api/companies";

        public CompanyService(HttpClient http) => _http = http;

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<CompanyResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _http.GetApiAsync<CompanyResponse>($"{Base}/{id}", ct);
        }

        public async Task<CompanyResponse?> GetWithVesselsAsync(int id, CancellationToken ct = default)
        {
            return await _http.GetApiAsync<CompanyResponse>($"{Base}/{id}/with-vessels", ct);
        }

        public async Task<IReadOnlyList<CompanyResponse>> GetAllAsync(CancellationToken ct = default)
        {
            return await _http.GetApiListAsync<CompanyResponse>(Base, ct);
          
        }

        public async Task<IReadOnlyList<CompanyResponse>> GetActiveAsync(CancellationToken ct = default)
        {
            return await _http.GetApiListAsync<CompanyResponse>($"{Base}/active", ct);
        }

        public async Task<IReadOnlyList<CompanyResponse>> SearchByNameAsync(string name, CancellationToken ct = default)
        {
            return await _http.GetApiListAsync<CompanyResponse>($"{Base}/search?name={Uri.EscapeDataString(name)}", ct);
        }

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<CompanyResponse> CreateAsync(string? registrationNumber, string companyName, int? countryId = null,
            string? contactName = null, string? email = null, string? phone = null, string? address = null,
            CancellationToken ct = default)
        {
            var request = new CreateCompanyRequest
            {
                RegistrationNumber = registrationNumber,
                CompanyName = companyName,
                CountryId = countryId,
                ContactName = contactName,
                Email = email,
                Phone = phone,
                Address = address
            };

            var http = await _http.PostAsJsonAsync(Base, request, ct);
            http.EnsureSuccessStatusCode();

            var response = await http.Content.ReadFromJsonAsync<ApiResponse<CompanyResponse>>(ct);
            return response!.Data!;
        }

        public async Task<IReadOnlyList<CompanyResponse>> CreateRangeAsync(IEnumerable<CreateCompanyRequest> commands, CancellationToken ct = default)
        {
            var http = await _http.PostAsJsonAsync($"{Base}/batch", commands, ct);
            http.EnsureSuccessStatusCode();

            var response = await http.Content.ReadFromJsonAsync<IReadOnlyList<CompanyResponse>>(ct);
            return response ?? [];
        }

        public async Task UpdateAsync(int id, string? registerationNumber, string companyName, int? countryId = null,
            string? contactName = null, string? email = null, string? phone = null, string? address = null,
            CancellationToken ct = default)
        {
            var request = new UpdateCompanyRequest
            {
                CompanyName = companyName,
                CountryId = countryId,
                ContactName = contactName,
                Email = email,
                Phone = phone,
                Address = address,
                RegistrationNumber = registerationNumber
                
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