using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.CurrencyDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using System.Net.Http.Json;

namespace Marilog.Client.Services.SystemServices
{
    public class CurrencyService : ICurrencyService
    {
        private readonly HttpClient _http;
        private const string Base = "api/currencies";

        public CurrencyService(HttpClient http) => _http = http;

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<CurrencyResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<CurrencyResponse>>($"{Base}/{id}", ct);
            return response?.Data;
        }

        public async Task<CurrencyResponse?> GetByCodeAsync(string code, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<CurrencyResponse>>($"{Base}/by-code/{Uri.EscapeDataString(code)}", ct);
            return response?.Data;
        }

        public async Task<CurrencyResponse?> GetBaseCurrencyAsync(CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<CurrencyResponse>>($"{Base}/base", ct);
            return response?.Data;
        }

        public async Task<IReadOnlyList<CurrencyResponse>> GetAllAsync(CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<IReadOnlyList<CurrencyResponse>>>(Base, ct);
            return response?.Data ?? [];
        }

        public async Task<IReadOnlyList<CurrencyResponse>> GetActiveAsync(CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<IReadOnlyList<CurrencyResponse>>>($"{Base}/active", ct);
            return response?.Data ?? [];
        }

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<CurrencyResponse> CreateAsync(string code, string name, decimal exchangeRate, string? symbol = null, CancellationToken ct = default)
        {
            var request = new CreateCurrencyRequest { Code = code, Name = name, ExchangeRate = exchangeRate, Symbol = symbol };
            var http = await _http.PostAsJsonAsync(Base, request, ct);
            http.EnsureSuccessStatusCode();
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<CurrencyResponse>>(ct);
            return response!.Data!;
        }

        public async Task<IReadOnlyList<CurrencyResponse>> CreateRangeAsync(IEnumerable<CreateCurrencyRequest> commands, CancellationToken ct = default)
        {
            var http = await _http.PostAsJsonAsync($"{Base}/batch", commands, ct);
            http.EnsureSuccessStatusCode();
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<CurrencyResponse>>>(ct);
            return response?.Data ?? [];
        }

        public async Task UpdateAsync(int id, string name, decimal exchangeRate, string? symbol = null, CancellationToken ct = default)
        {
            var request = new UpdateCurrencyRequest { Name = name, ExchangeRate = exchangeRate, Symbol = symbol };
            var http = await _http.PutAsJsonAsync($"{Base}/{id}", request, ct);
            http.EnsureSuccessStatusCode();
        }

        public async Task UpdateRateAsync(int id, decimal newRate, CancellationToken ct = default)
        {
            var request = new UpdateCurrencyRateRequest { NewRate = newRate };
            var http = await _http.PatchAsJsonAsync($"{Base}/{id}/rate", request, ct);
            http.EnsureSuccessStatusCode();
        }

        public async Task SetAsBaseAsync(int id, CancellationToken ct = default)
        {
            var http = await _http.PatchAsync($"{Base}/{id}/set-base", null, ct);
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