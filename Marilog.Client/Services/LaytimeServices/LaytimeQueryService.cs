using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.CharterLaytimeServices;
using System.Net.Http.Json;

namespace Marilog.Client.Services.LaytimeServices
{
    public sealed class LaytimeQueryService : ILaytimeQueryService
    {
        private readonly HttpClient _http;
        public LaytimeQueryService(HttpClient http) => _http = http;

        public async Task<IReadOnlyList<LaytimeSegmentResponse>> GetSegmentsAsync(int calculationId, CancellationToken ct = default)
        {
            var result = await _http.GetFromJsonAsync<ApiResponse<IReadOnlyList<LaytimeSegmentResponse>>>(
                $"api/laytime-calculations/{calculationId}/segments", ct);
            return result?.Data ?? Array.Empty<LaytimeSegmentResponse>();
        }
    }
}
