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


        // 4×4 grid, 1° apart, with no point on the vessel itself: offsets -1.5, -0.5, +0.5, +1.5
        private static readonly double[] WindGridOffsets = [-1.75, -0.75, 0.75, 1.75];

        public async Task<IReadOnlyList<WindFieldPoint>> GetWindFieldAsync(double latitude, double longitude, CancellationToken ct = default)
        {
            var lat = Math.Round(latitude, 2);
            var lon = Math.Round(longitude, 2);
            var key = $"wind-field:{lat.ToString(CultureInfo.InvariantCulture)}:{lon.ToString(CultureInfo.InvariantCulture)}";

            if (_cache.TryGetValue(key, out IReadOnlyList<WindFieldPoint>? cached) && cached is not null)
                return cached;

            var result = await _provider.GetCurrentWindAsync(BuildWindGrid(lat, lon), ct);
            _cache.Set(key, result, result.Count == 0 ? MissTtl : HitTtl);
            return result;
        }

        private static IReadOnlyList<(double Latitude, double Longitude)> BuildWindGrid(double lat, double lon)
        {
            var points = new List<(double, double)>(WindGridOffsets.Length * WindGridOffsets.Length);
            foreach (var dLat in WindGridOffsets)
            {
                var pLat = lat + dLat;
                if (pLat is < -85 or > 85) continue;

                foreach (var dLon in WindGridOffsets)
                    points.Add((Math.Round(pLat, 2), NormalizeLongitude(lon + dLon)));
            }
            return points;
        }

        private static double NormalizeLongitude(double lon) => Math.Round(((lon + 540) % 360) - 180, 2);
    }
}