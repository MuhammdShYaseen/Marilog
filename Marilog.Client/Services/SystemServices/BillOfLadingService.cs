using Marilog.Contracts.DTOs.Requests.BillOfLadingDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using System.Net.Http.Json;

namespace Marilog.Client.Services.SystemServices
{
    public class BillOfLadingService : IBillOfLadingService
    {
        private readonly HttpClient _http;
        private const string Base = "api/billoflading";

        public BillOfLadingService(HttpClient http)
        {
            _http = http;
        }

        // ── Queries ──────────────────────────────────────────────────────────
        public async Task<BillOfLadingResponse> GetByIdAsync(int id, CancellationToken ct = default)
        { 
            var bl = await _http.GetFromJsonAsync<BillOfLadingResponse>($"{Base}/{id}", ct);
            if(bl == null)
                return new BillOfLadingResponse();

            return bl;
        }

        public async Task<IReadOnlyList<BillOfLadingResponse>> GetByVoyageAsync(int voyageId, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<IReadOnlyList<BillOfLadingResponse>>($"{Base}/voyage/{voyageId}", ct);
            return response ?? [];   
        }

        // ── Commands ─────────────────────────────────────────────────────────
        public async Task<BillOfLadingResponse> CreateAsync(CreateBillOfLadingRequest request, CancellationToken ct = default)
        {
            var http = await _http.PostAsJsonAsync(Base, request, ct);
            http.EnsureSuccessStatusCode();
            var response = await http.Content.ReadFromJsonAsync<BillOfLadingResponse>(ct);
            return response!;
        }

        public async Task<BillOfLadingResponse> UpdateAsync(int id, UpdateBillOfLadingRequest request, CancellationToken ct = default)
        {
            var http = await _http.PutAsJsonAsync($"{Base}/{id}", request, ct);
            http.EnsureSuccessStatusCode();
            var response = await http.Content.ReadFromJsonAsync<BillOfLadingResponse>(ct);
            
            return response!;
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var http = await _http.DeleteAsync($"{Base}/{id}", ct);
            http.EnsureSuccessStatusCode();
        }

        // ── Sensitive Operations ─────────────────────────────────────────────
        public async Task<BillOfLadingResponse> ChangeBlNumberAsync(int id, ChangeBlNumberRequest request, CancellationToken ct = default)
        {
            var http = await _http.PatchAsJsonAsync($"{Base}/{id}/bl-number", request, ct);
            http.EnsureSuccessStatusCode();
            var response = await http.Content.ReadFromJsonAsync<BillOfLadingResponse>(ct);
            return response!;
        }

        public async Task<BillOfLadingResponse> ChangeIssuanceTypeAsync(int id, ChangeIssuanceTypeRequest request, CancellationToken ct = default)
        {
            var http = await _http.PatchAsJsonAsync($"{Base}/{id}/issuance-type", request, ct);
            http.EnsureSuccessStatusCode();
            var response = await http.Content.ReadFromJsonAsync<BillOfLadingResponse>(ct);
            return response!;
        }

        public async Task<BillOfLadingResponse> LinkToMasterBlAsync(int id, LinkToMasterBlRequest request, CancellationToken ct = default)
        {
            var http = await _http.PatchAsJsonAsync($"{Base}/{id}/link-master-bl", request, ct);
            http.EnsureSuccessStatusCode();
            var response = await http.Content.ReadFromJsonAsync<BillOfLadingResponse>(ct);
            return response!;
        }
    }
}