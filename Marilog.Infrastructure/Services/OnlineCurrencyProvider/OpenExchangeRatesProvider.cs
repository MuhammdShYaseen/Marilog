using Marilog.Application.Interfaces.OnlineCurrencyRate;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http.Json;

namespace Marilog.Infrastructure.Services.OnlineCurrencyProvider
{
    public sealed class OpenExchangeRatesProvider : IExchangeRateProvider
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

        private readonly HttpClient _http;
        private readonly IMemoryCache _cache;

        public OpenExchangeRatesProvider(HttpClient http, IMemoryCache cache)
        {
            _http = http;
            _cache = cache;
        }

        public Task<IReadOnlyDictionary<string, decimal>> GetLatestAsync(IEnumerable<string> codes, CancellationToken ct = default)
            => GetAsync("latest.json", codes, CacheDuration, ct);

        public Task<IReadOnlyDictionary<string, decimal>> GetHistoricalAsync(DateOnly date, IEnumerable<string> codes, CancellationToken ct = default)
        {
            // A past day's rates never change; today's are still moving.
            var isPastDay = date < DateOnly.FromDateTime(DateTime.UtcNow);
            var duration = isPastDay ? TimeSpan.FromDays(1) : CacheDuration;

            return GetAsync($"historical/{date:yyyy-MM-dd}.json", codes, duration, ct);
        }
        private async Task<IReadOnlyDictionary<string, decimal>> GetAsync(string path, IEnumerable<string> codes, TimeSpan cacheDuration, CancellationToken ct)
        {
            var symbols = string.Join(",", codes
                .Select(c => c.ToUpperInvariant())
                .Distinct()
                .OrderBy(c => c));

            if (symbols.Length == 0)
                return new Dictionary<string, decimal>();

            return await _cache.GetOrCreateAsync($"oxr:{path}:{symbols}", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = cacheDuration;
                return FetchAsync(path, symbols, ct);
            }) ?? throw new InvalidOperationException("Exchange rates cache returned no data.");
        }


        //====Privates===============================================
        private async Task<IReadOnlyDictionary<string, decimal>> FetchAsync(string path, string symbols, CancellationToken ct)
        {
            if (_http.BaseAddress is null || _http.DefaultRequestHeaders.Authorization is null)
                throw new InvalidOperationException("Exchange rate provider is not configured (ExchangeRates:BaseUrl / AppId).");

            var response = await _http.GetFromJsonAsync<OxrResponse>($"{path}?symbols={symbols}", ct)
                ?? throw new InvalidOperationException("Empty response from Open Exchange Rates.");

            if (!string.Equals(response.Base, "USD", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Unexpected base currency '{response.Base}'.");

            // OXR: units per 1 USD  ->  Marilog: USD per 1 unit
            return response.Rates
                .Where(r => r.Value > 0)
                .ToDictionary(r => r.Key, r => Math.Round(1m / r.Value, 8));
        }

        private sealed record OxrResponse(string Base, Dictionary<string, decimal> Rates);
    }
}