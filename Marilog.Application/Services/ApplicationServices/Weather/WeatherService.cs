using System.Globalization;
using Marilog.Application.Interfaces.Weather;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Microsoft.Extensions.Caching.Memory;

namespace Marilog.Application.Services.ApplicationServices.SystemServices
{
    public sealed class WeatherService : IWeatherService
    {
        private static readonly TimeSpan HitTtl = TimeSpan.FromMinutes(30);
        private static readonly TimeSpan MissTtl = TimeSpan.FromMinutes(5);

        private readonly IWeatherProvider _provider;
        private readonly IMemoryCache _cache;

        public WeatherService(IWeatherProvider provider, IMemoryCache cache)
        {
            _provider = provider;
            _cache = cache;
        }

        public async Task<WeatherResponse?> GetAtAsync(double latitude, double longitude, CancellationToken ct = default)
        {
            var lat = Math.Round(latitude, 2);
            var lon = Math.Round(longitude, 2);
            var key = $"weather:{lat.ToString(CultureInfo.InvariantCulture)}:{lon.ToString(CultureInfo.InvariantCulture)}";

            if (_cache.TryGetValue(key, out WeatherResponse? cached))
                return cached;

            var result = await _provider.GetAtAsync(lat, lon, ct);
            _cache.Set(key, result, result is null ? MissTtl : HitTtl);
            return result;
        }
    }
}