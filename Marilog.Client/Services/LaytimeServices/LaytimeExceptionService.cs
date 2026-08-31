using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.LayTimeDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.CharterLaytimeServices;
using System.Net.Http.Json;

namespace Marilog.Client.Services.LaytimeServices
{
    public sealed class LaytimeExceptionService : ILaytimeExceptionService
    {
        private static string Base(int calculationId) => $"api/laytime-calculations/{calculationId}/exceptions";
        private readonly HttpClient _http;
        public LaytimeExceptionService(HttpClient http) => _http = http;

        public async Task<LaytimeExceptionResponse> AddExceptionAsync(int calculationId, AddLaytimeExceptionRequest request, CancellationToken ct = default)
        {
            var response = await _http.PostAsJsonAsync(Base(calculationId), request, ct);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<LaytimeExceptionResponse>>(cancellationToken: ct);
            return result?.Data ?? throw new InvalidOperationException("Add exception failed.");
        }

        public async Task<IReadOnlyList<LaytimeExceptionResponse>> GetExceptionsAsync(int calculationId, CancellationToken ct = default)
        {
            var result = await _http.GetFromJsonAsync<ApiResponse<IReadOnlyList<LaytimeExceptionResponse>>>(Base(calculationId), ct);
            return result?.Data ?? Array.Empty<LaytimeExceptionResponse>();
        }

        public async Task UpdateExceptionAsync(int exceptionId, UpdateLaytimeExceptionRequest request, CancellationToken ct = default)
        {
            var response = await _http.PutAsJsonAsync($"{Base(exceptionId)}", request, ct);
            response.EnsureSuccessStatusCode();
        }

        public async Task RemoveExceptionAsync(int calculationId, int exceptionId, CancellationToken ct = default)
        {
            var response = await _http.DeleteAsync($"{Base(calculationId)}/{exceptionId}", ct);
            response.EnsureSuccessStatusCode();
        }
    }
}
