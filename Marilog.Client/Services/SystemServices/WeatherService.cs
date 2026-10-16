using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;

namespace Marilog.Client.Services.SystemServices
{
    public class WeatherService : IWeatherService
    {
        private readonly HttpClient _http;

        public WeatherService(HttpClient http) => _http = http;

        public async Task<WeatherResponse?> GetAtAsync(double latitude, double longitude, CancellationToken ct = default)
        {
            var url = $"api/weather?lat={latitude.ToString(CultureInfo.InvariantCulture)}" +
                      $"&lon={longitude.ToString(CultureInfo.InvariantCulture)}";

            var http = await _http.GetAsync(url, ct);
            if (http.StatusCode == HttpStatusCode.NotFound) return null;
            http.EnsureSuccessStatusCode();

            var response = await http.Content.ReadFromJsonAsync<ApiResponse<WeatherResponse>>(ct);
            return response?.Data;
        }

        public async Task<IReadOnlyList<WindFieldPoint>> GetWindFieldAsync(double latitude, double longitude, CancellationToken ct = default)
        {
            var url = $"api/weather/wind-field?lat={latitude.ToString(CultureInfo.InvariantCulture)}" +
                      $"&lon={longitude.ToString(CultureInfo.InvariantCulture)}";

            var response = await _http.GetFromJsonAsync<ApiResponse<IReadOnlyList<WindFieldPoint>>>(url, ct);
            return response?.Data ?? [];
        }
    }
}