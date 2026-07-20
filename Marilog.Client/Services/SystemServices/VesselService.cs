using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.Common;
using Marilog.Contracts.DTOs.Requests.VesselDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using System.Net.Http.Json;

namespace Marilog.Client.Services.SystemServices
{
    public class VesselService : IVesselService
    {
        private readonly HttpClient _http;
        private const string Base = "api/vessels";

        public VesselService(HttpClient http) => _http = http;

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<VesselResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<VesselResponse>>($"{Base}/{id}", ct);
            return response?.Data;
        }

        public async Task<VesselResponse?> GetByImoAsync(string imoNumber, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<VesselResponse>>($"{Base}/by-imo/{Uri.EscapeDataString(imoNumber)}", ct);
            return response?.Data;
        }

        public async Task<IReadOnlyList<VesselResponse>> GetAllAsync(CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<IReadOnlyList<VesselResponse>>>(Base, ct);
            return response?.Data ?? [];
        }

        public async Task<IReadOnlyList<VesselResponse>> GetActiveAsync(CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<IReadOnlyList<VesselResponse>>>($"{Base}/active", ct);
            return response?.Data ?? [];
        }

        public async Task<IReadOnlyList<VesselResponse>> GetByCompanyAsync(int companyId, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<IReadOnlyList<VesselResponse>>>($"{Base}/by-company/{companyId}", ct);
            return response?.Data ?? [];
        }

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<VesselResponse> CreateAsync(int companyId, string vesselName,
            string? imoNumber = null, decimal? grossTonnage = null,
            int? flagCountryId = null, string? notes = null,
            CancellationToken ct = default)
        {
            var request = new CreateVesselRequest(companyId, vesselName, imoNumber, grossTonnage, flagCountryId, notes);
            var http = await _http.PostAsJsonAsync(Base, request, ct);
            http.EnsureSuccessStatusCode();
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<VesselResponse>>(ct);
            return response!.Data!;
        }

        public async Task<IReadOnlyList<VesselResponse>> CreateRangeAsync(IEnumerable<CreateVesselRequest> commands, CancellationToken ct = default)
        {
            var http = await _http.PostAsJsonAsync($"{Base}/batch", commands, ct);
            http.EnsureSuccessStatusCode();
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<VesselResponse>>>(ct);
            return response?.Data ?? [];
        }

        public async Task UpdateAsync(int id, string vesselName,
            string? imoNumber = null, decimal? grossTonnage = null,
            int? flagCountryId = null, string? notes = null,
            CancellationToken ct = default)
        {
            var request = new UpdateVesselRequest(vesselName, imoNumber, grossTonnage, flagCountryId, notes);
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

        // ── Certificates ─────────────────────────────────────────────────────────────

        public async Task AddCertificateAsync(int vesselId, UpsertCertificateRequest request, CancellationToken ct = default)
        {
            var http = await _http.PostAsJsonAsync($"{Base}/{vesselId}/certificates", request, ct);
            http.EnsureSuccessStatusCode();
        }

        public async Task UpdateCertificateAsync(int vesselId, int index, UpsertCertificateRequest request, CancellationToken ct = default)
        {
            var http = await _http.PutAsJsonAsync($"{Base}/{vesselId}/certificates/{index}", request, ct);
            http.EnsureSuccessStatusCode();
        }

        public async Task RemoveCertificateAsync(int vesselId, int index,
            CancellationToken ct = default)
        {
            var http = await _http.DeleteAsync($"{Base}/{vesselId}/certificates/{index}", ct);
            http.EnsureSuccessStatusCode();
        }
    }
}