using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.LayTimeDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.CharterLaytimeServices;
using System.Net.Http.Json;

namespace Marilog.Client.Services.LaytimeServices
{
    public sealed class SofEventService : ISofEventService
    {
        private static string Base(int calculationId) => $"api/laytime-calculations/{calculationId}/sof-events";
        private readonly HttpClient _http;
        public SofEventService(HttpClient http) => _http = http;

        public async Task<SofEventResponse> AddSofEventAsync(int calculationId, AddSofEventRequest request, CancellationToken ct = default)
        {
            var response = await _http.PostAsJsonAsync(Base(calculationId), request, ct);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<SofEventResponse>>(cancellationToken: ct);
            return result?.Data ?? throw new InvalidOperationException("Add SOF event failed.");
        }

        public async Task<IReadOnlyList<SofEventResponse>> AddSofEventsBatchAsync(int calculationId, IEnumerable<AddSofEventRequest> requests, CancellationToken ct = default)
        {
            var response = await _http.PostAsJsonAsync($"{Base(calculationId)}/batch", requests, ct);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<SofEventResponse>>>(cancellationToken: ct);
            return result?.Data ?? Array.Empty<SofEventResponse>();
        }

        public async Task<IReadOnlyList<SofEventResponse>> GetSofEventsAsync(int calculationId, CancellationToken ct = default)
        {
            var result = await _http.GetFromJsonAsync<ApiResponse<IReadOnlyList<SofEventResponse>>>(Base(calculationId), ct);
            return result?.Data ?? Array.Empty<SofEventResponse>();
        }

        public async Task UpdateSofEventImpactAsync(int sofEventId, UpdateSofEventImpactRequest request, CancellationToken ct = default)
        {
            var response = await _http.PutAsJsonAsync($"{Base(sofEventId)}/impact", request, ct);
            response.EnsureSuccessStatusCode();
        }

        public async Task RemoveSofEventAsync(int calculationId, int sofEventId, CancellationToken ct = default)
        {
            var response = await _http.DeleteAsync($"{Base(calculationId)}/{sofEventId}", ct);
            response.EnsureSuccessStatusCode();
        }
    }
}
