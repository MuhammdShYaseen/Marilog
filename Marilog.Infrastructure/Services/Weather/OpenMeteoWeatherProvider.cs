using Marilog.Application.Interfaces.Weather;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Infrastructure.Models.Weather;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Marilog.Infrastructure.Services.Weather
{
    internal sealed class OpenMeteoWeatherProvider : IWeatherProvider
    {
        private const int ForecastDays = 5;
        private const double KmhToKnots = 1 / 1.852;
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        private readonly HttpClient _http;
        private readonly OpenMeteoOptions _options;
        private readonly ILogger<OpenMeteoWeatherProvider> _logger;

        public OpenMeteoWeatherProvider(HttpClient http, IOptions<OpenMeteoOptions> options,
            ILogger<OpenMeteoWeatherProvider> logger)
        {
            _http = http;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<WeatherResponse?> GetAtAsync(double latitude, double longitude, CancellationToken ct = default)
        {
            var forecastTask = GetJsonAsync<ForecastDto>(BuildForecastUrl(latitude, longitude), ct);
            var marineTask = GetJsonAsync<MarineDto>(BuildMarineUrl(latitude, longitude), ct);
            await Task.WhenAll(forecastTask, marineTask);

            var forecast = forecastTask.Result;
            if (forecast?.Current is null) return null;

            // Sea state is optional: points near the coast or inland can come back without wave data.
            return Map(forecast, marineTask.Result);
        }

        public async Task<IReadOnlyList<WindFieldPoint>> GetCurrentWindAsync(
    IReadOnlyList<(double Latitude, double Longitude)> points, CancellationToken ct = default)
        {
            // Open-Meteo answers a multi-location request with a JSON array in request order.
            // A single location comes back as a plain object, so this method expects two or more points.
            if (points.Count < 2) return [];

            var lats = string.Join(",", points.Select(p => p.Latitude.ToString(Inv)));
            var lons = string.Join(",", points.Select(p => p.Longitude.ToString(Inv)));

            var url = AppendKey(
                $"{_options.ForecastBaseUrl.TrimEnd('/')}/v1/forecast" +
                $"?latitude={lats}&longitude={lons}" +
                "&timezone=GMT&wind_speed_unit=kn&current=wind_speed_10m,wind_direction_10m");

            var results = await GetJsonAsync<List<ForecastDto>>(url, ct);
            if (results is null || results.Count != points.Count) return [];

            var field = new List<WindFieldPoint>(points.Count);
            for (var i = 0; i < points.Count; i++)
            {
                var c = results[i].Current;
                field.Add(new WindFieldPoint(
                    points[i].Latitude,
                    points[i].Longitude,
                    c?.WindSpeed,
                    c?.WindDirection,
                    Beaufort(c?.WindSpeed)));
            }
            return field;
        }

        // ── Requests ─────────────────────────────────────────────────────────

        private string BuildForecastUrl(double lat, double lon) => AppendKey(
            $"{_options.ForecastBaseUrl.TrimEnd('/')}/v1/forecast" +
            $"?latitude={lat.ToString(Inv)}&longitude={lon.ToString(Inv)}" +
            $"&timezone=GMT&wind_speed_unit=kn&forecast_days={ForecastDays}" +
            "&current=temperature_2m,relative_humidity_2m,weather_code,pressure_msl," +
            "wind_speed_10m,wind_direction_10m,wind_gusts_10m,visibility" +
            "&daily=weather_code,temperature_2m_max,temperature_2m_min,precipitation_sum," +
            "wind_speed_10m_max,wind_gusts_10m_max,wind_direction_10m_dominant");

        private string BuildMarineUrl(double lat, double lon) => AppendKey(
            $"{_options.MarineBaseUrl.TrimEnd('/')}/v1/marine" +
            $"?latitude={lat.ToString(Inv)}&longitude={lon.ToString(Inv)}" +
            $"&timezone=GMT&cell_selection=sea&forecast_days={ForecastDays}" +
            "&current=wave_height,wave_direction,wave_period,swell_wave_height,swell_wave_direction," +
            "swell_wave_period,wind_wave_height,sea_surface_temperature,ocean_current_velocity,ocean_current_direction" +
            "&daily=wave_height_max,wave_direction_dominant,swell_wave_height_max");

        private string AppendKey(string url) =>
            string.IsNullOrWhiteSpace(_options.ApiKey) ? url : $"{url}&apikey={Uri.EscapeDataString(_options.ApiKey)}";

        private async Task<T?> GetJsonAsync<T>(string url, CancellationToken ct) where T : class
        {
            try
            {
                using var response = await _http.GetAsync(url, ct);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Open-Meteo returned {StatusCode} for {Type}", (int)response.StatusCode, typeof(T).Name);
                    return null;
                }
                return await response.Content.ReadFromJsonAsync<T>(ct);
            }
            catch (Exception ex) when (ex is HttpRequestException or JsonException
                                       || (ex is TaskCanceledException && !ct.IsCancellationRequested))
            {
                _logger.LogWarning(ex, "Open-Meteo request failed for {Type}", typeof(T).Name);
                return null;
            }
        }

        // ── Mapping ──────────────────────────────────────────────────────────

        private static WeatherResponse Map(ForecastDto forecast, MarineDto? marine)
        {
            var c = forecast.Current!;

            var current = new CurrentWeather(
                TemperatureC: c.Temperature,
                HumidityPercent: c.Humidity,
                PressureHpa: c.Pressure,
                VisibilityM: c.Visibility,
                WeatherCode: c.WeatherCode,
                Description: Describe(c.WeatherCode),
                WindSpeedKn: c.WindSpeed,
                WindDirectionDeg: c.WindDirection,
                WindGustsKn: c.WindGusts,
                Beaufort: Beaufort(c.WindSpeed));

            CurrentSeaState? sea = null;
            if (marine?.Current is { } m && (m.WaveHeight is not null || m.SeaSurfaceTemperature is not null))
            {
                sea = new CurrentSeaState(
                    WaveHeightM: m.WaveHeight,
                    WaveDirectionDeg: m.WaveDirection,
                    WavePeriodS: m.WavePeriod,
                    SwellHeightM: m.SwellHeight,
                    SwellDirectionDeg: m.SwellDirection,
                    SwellPeriodS: m.SwellPeriod,
                    WindWaveHeightM: m.WindWaveHeight,
                    SeaSurfaceTemperatureC: m.SeaSurfaceTemperature,
                    CurrentSpeedKn: m.CurrentVelocityKmh is double v ? Math.Round(v * KmhToKnots, 1) : null,
                    CurrentDirectionDeg: m.CurrentDirection);
            }

            return new WeatherResponse(
                Latitude: forecast.Latitude,
                Longitude: forecast.Longitude,
                ModelTimeUtc: ParseUtc(c.Time),
                Current: current,
                Sea: sea,
                Daily: MapDaily(forecast.Daily, marine?.Daily));
        }

        private static IReadOnlyList<DailyWeather> MapDaily(ForecastDailyDto? f, MarineDailyDto? m)
        {
            if (f is null || f.Time.Count == 0) return [];

            // Match marine days by date rather than by index, in case the two sources start on different days.
            var marineIndex = new Dictionary<string, int>();
            if (m is not null)
                for (var j = 0; j < m.Time.Count; j++) marineIndex[m.Time[j]] = j;

            var days = new List<DailyWeather>(f.Time.Count);
            for (var i = 0; i < f.Time.Count; i++)
            {
                if (!DateOnly.TryParseExact(f.Time[i], "yyyy-MM-dd", Inv, DateTimeStyles.None, out var date))
                    continue;

                var hasSea = marineIndex.TryGetValue(f.Time[i], out var j);
                var code = At(f.WeatherCode, i);
                var windMax = At(f.WindSpeedMax, i);

                days.Add(new DailyWeather(
                    Date: date,
                    WeatherCode: code,
                    Description: Describe(code),
                    TemperatureMinC: At(f.TemperatureMin, i),
                    TemperatureMaxC: At(f.TemperatureMax, i),
                    PrecipitationMm: At(f.PrecipitationSum, i),
                    WindSpeedMaxKn: windMax,
                    WindGustsMaxKn: At(f.WindGustsMax, i),
                    WindDirectionDominantDeg: At(f.WindDirectionDominant, i),
                    BeaufortMax: Beaufort(windMax),
                    WaveHeightMaxM: hasSea ? At(m!.WaveHeightMax, j) : null,
                    SwellHeightMaxM: hasSea ? At(m!.SwellHeightMax, j) : null,
                    WaveDirectionDominantDeg: hasSea ? At(m!.WaveDirectionDominant, j) : null));
            }
            return days;
        }

        private static T? At<T>(List<T?> list, int i) where T : struct => i < list.Count ? list[i] : null;

        private static DateTime? ParseUtc(string? value) =>
            DateTime.TryParseExact(value, "yyyy-MM-dd'T'HH:mm", Inv,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var t) ? t : null;

        // Beaufort upper bounds in knots for forces 0..11; anything above is force 12.
        private static readonly double[] BeaufortBounds = [1, 4, 7, 11, 17, 22, 28, 34, 41, 48, 56, 64];

        private static int? Beaufort(double? knots)
        {
            if (knots is not double kn) return null;
            for (var i = 0; i < BeaufortBounds.Length; i++)
                if (kn < BeaufortBounds[i]) return i;
            return 12;
        }

        private static readonly Dictionary<int, string> WmoCodes = new()
        {
            [0] = "Clear sky",
            [1] = "Mainly clear",
            [2] = "Partly cloudy",
            [3] = "Overcast",
            [45] = "Fog",
            [48] = "Depositing rime fog",
            [51] = "Light drizzle",
            [53] = "Drizzle",
            [55] = "Dense drizzle",
            [56] = "Light freezing drizzle",
            [57] = "Freezing drizzle",
            [61] = "Light rain",
            [63] = "Rain",
            [65] = "Heavy rain",
            [66] = "Light freezing rain",
            [67] = "Freezing rain",
            [71] = "Light snow",
            [73] = "Snow",
            [75] = "Heavy snow",
            [77] = "Snow grains",
            [80] = "Light rain showers",
            [81] = "Rain showers",
            [82] = "Violent rain showers",
            [85] = "Light snow showers",
            [86] = "Snow showers",
            [95] = "Thunderstorm",
            [96] = "Thunderstorm with hail",
            [99] = "Thunderstorm with heavy hail"
        };

        private static string? Describe(int? code) =>
            code is int c && WmoCodes.TryGetValue(c, out var text) ? text : null;

        // ── Source payloads ──────────────────────────────────────────────────

        private sealed class ForecastDto
        {
            [JsonPropertyName("latitude")] public double Latitude { get; set; }
            [JsonPropertyName("longitude")] public double Longitude { get; set; }
            [JsonPropertyName("current")] public ForecastCurrentDto? Current { get; set; }
            [JsonPropertyName("daily")] public ForecastDailyDto? Daily { get; set; }
        }

        private sealed class ForecastCurrentDto
        {
            [JsonPropertyName("time")] public string? Time { get; set; }
            [JsonPropertyName("temperature_2m")] public double? Temperature { get; set; }
            [JsonPropertyName("relative_humidity_2m")] public double? Humidity { get; set; }
            [JsonPropertyName("weather_code")] public int? WeatherCode { get; set; }
            [JsonPropertyName("pressure_msl")] public double? Pressure { get; set; }
            [JsonPropertyName("wind_speed_10m")] public double? WindSpeed { get; set; }
            [JsonPropertyName("wind_direction_10m")] public double? WindDirection { get; set; }
            [JsonPropertyName("wind_gusts_10m")] public double? WindGusts { get; set; }
            [JsonPropertyName("visibility")] public double? Visibility { get; set; }
        }

        private sealed class ForecastDailyDto
        {
            [JsonPropertyName("time")] public List<string> Time { get; set; } = [];
            [JsonPropertyName("weather_code")] public List<int?> WeatherCode { get; set; } = [];
            [JsonPropertyName("temperature_2m_max")] public List<double?> TemperatureMax { get; set; } = [];
            [JsonPropertyName("temperature_2m_min")] public List<double?> TemperatureMin { get; set; } = [];
            [JsonPropertyName("precipitation_sum")] public List<double?> PrecipitationSum { get; set; } = [];
            [JsonPropertyName("wind_speed_10m_max")] public List<double?> WindSpeedMax { get; set; } = [];
            [JsonPropertyName("wind_gusts_10m_max")] public List<double?> WindGustsMax { get; set; } = [];
            [JsonPropertyName("wind_direction_10m_dominant")] public List<double?> WindDirectionDominant { get; set; } = [];
        }

        private sealed class MarineDto
        {
            [JsonPropertyName("current")] public MarineCurrentDto? Current { get; set; }
            [JsonPropertyName("daily")] public MarineDailyDto? Daily { get; set; }
        }

        private sealed class MarineCurrentDto
        {
            [JsonPropertyName("wave_height")] public double? WaveHeight { get; set; }
            [JsonPropertyName("wave_direction")] public double? WaveDirection { get; set; }
            [JsonPropertyName("wave_period")] public double? WavePeriod { get; set; }
            [JsonPropertyName("swell_wave_height")] public double? SwellHeight { get; set; }
            [JsonPropertyName("swell_wave_direction")] public double? SwellDirection { get; set; }
            [JsonPropertyName("swell_wave_period")] public double? SwellPeriod { get; set; }
            [JsonPropertyName("wind_wave_height")] public double? WindWaveHeight { get; set; }
            [JsonPropertyName("sea_surface_temperature")] public double? SeaSurfaceTemperature { get; set; }
            [JsonPropertyName("ocean_current_velocity")] public double? CurrentVelocityKmh { get; set; }
            [JsonPropertyName("ocean_current_direction")] public double? CurrentDirection { get; set; }
        }

        private sealed class MarineDailyDto
        {
            [JsonPropertyName("time")] public List<string> Time { get; set; } = [];
            [JsonPropertyName("wave_height_max")] public List<double?> WaveHeightMax { get; set; } = [];
            [JsonPropertyName("wave_direction_dominant")] public List<double?> WaveDirectionDominant { get; set; } = [];
            [JsonPropertyName("swell_wave_height_max")] public List<double?> SwellHeightMax { get; set; } = [];
        }
    }
}