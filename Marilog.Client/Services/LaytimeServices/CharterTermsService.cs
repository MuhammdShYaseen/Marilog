using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.LayTimeDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.CharterLaytimeServices;
using System.Net.Http.Json;

namespace Marilog.Client.Services.LaytimeServices
{
    public sealed class CharterTermsService : ICharterTermsService
    {
        private const string Base = "api/charter-terms";
        private readonly HttpClient _http;
        public CharterTermsService(HttpClient http) => _http = http;

        public async Task<CharterTermsResponse> InitializeCharterTermsAsync(InitializeCharterTermsRequest request, CancellationToken ct = default)
        {
            var response = await _http.PostAsJsonAsync(Base, request, ct);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<CharterTermsResponse>>(cancellationToken: ct);
            return result?.Data ?? throw new InvalidOperationException("Initialize charter terms failed.");
        }

        public async Task<CharterTermsResponse?> GetCharterTermsAsync(int contractId, CancellationToken ct = default)
        {
            var response = await _http.GetAsync($"{Base}/{contractId}", ct);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<CharterTermsResponse>>(cancellationToken: ct);
            return result?.Data;
        }

        public async Task UpdateLoadingTermsAsync(int contractId, CargoOperationTermsRequest request, CancellationToken ct = default)
        {
            var response = await _http.PutAsJsonAsync($"{Base}/{contractId}/loading", request, ct);
            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateDischargingTermsAsync(int contractId, CargoOperationTermsRequest request, CancellationToken ct = default)
        {
            var response = await _http.PutAsJsonAsync($"{Base}/{contractId}/discharging", request, ct);
            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateDemurrageTermsAsync(int contractId, DemurrageTermsRequest request, CancellationToken ct = default)
        {
            var response = await _http.PutAsJsonAsync($"{Base}/{contractId}/demurrage", request, ct);
            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateDespatchTermsAsync(int contractId, DespatchTermsRequest request, CancellationToken ct = default)
        {
            var response = await _http.PutAsJsonAsync($"{Base}/{contractId}/despatch", request, ct);
            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateRuleOptionsAsync(int contractId, LaytimeRuleOptionsRequest request, CancellationToken ct = default)
        {
            var response = await _http.PutAsJsonAsync($"{Base}/{contractId}/rule-options", request, ct);
            response.EnsureSuccessStatusCode();
        }

    }
}
