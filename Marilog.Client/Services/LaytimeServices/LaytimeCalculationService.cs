using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.LayTimeDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.CharterLaytimeServices;
using System.Net.Http.Json;

namespace Marilog.Client.Services.LaytimeServices
{
    public sealed class LaytimeCalculationService : ILaytimeCalculationService
    {
        private const string Base = "api/LaytimeCalculation";
        private readonly HttpClient _http;
        public LaytimeCalculationService(HttpClient http) => _http = http;

        public async Task<LaytimeCalculationResponse> CreateCalculationAsync(CreateLaytimeCalculationRequest request, CancellationToken ct = default)
        {
            var response = await _http.PostAsJsonAsync(Base, request, ct);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<LaytimeCalculationResponse>>(cancellationToken: ct);
            return result?.Data ?? throw new InvalidOperationException("Create calculation failed.");
        }

        public async Task<LaytimeCalculationResponse?> GetCalculationAsync(int calculationId, CancellationToken ct = default)
        {
            var response = await _http.GetAsync($"{Base}/{calculationId}", ct);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<LaytimeCalculationResponse>>(cancellationToken: ct);
            return result?.Data;
        }

        public async Task<IReadOnlyList<LaytimeCalculationSummaryResponse>> GetCalculationsByVoyageAsync(int voyageId, CancellationToken ct = default)
        {
            var result = await _http.GetFromJsonAsync<ApiResponse<IReadOnlyList<LaytimeCalculationSummaryResponse>>>($"{Base}/voyage/{voyageId}", ct);
            return result?.Data ?? Array.Empty<LaytimeCalculationSummaryResponse>();
        }

        public async Task<LaytimeResultResponse> ComputeAsync(int calculationId, CancellationToken ct = default)
        {
            var response = await _http.PostAsync($"{Base}/{calculationId}/compute", null, ct);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<LaytimeResultResponse>>(cancellationToken: ct);
            return result?.Data ?? throw new InvalidOperationException("Compute failed.");
        }

        public async Task<LaytimeResultResponse> RecomputeAsync(int calculationId, CancellationToken ct = default)
        {
            var response = await _http.PostAsync($"{Base}/{calculationId}/recompute", null, ct);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<LaytimeResultResponse>>(cancellationToken: ct);
            return result?.Data ?? throw new InvalidOperationException("Recompute failed.");
        }

        public async Task FinalizeAsync(int calculationId, CancellationToken ct = default)
        {
            var response = await _http.PostAsync($"{Base}/{calculationId}/finalize", null, ct);
            response.EnsureSuccessStatusCode();
        }
    }
}
