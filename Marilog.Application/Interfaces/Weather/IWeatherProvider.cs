using Marilog.Contracts.DTOs.Responses;

namespace Marilog.Application.Interfaces.Weather
{
    public interface IWeatherProvider
    {
        /// <summary>Returns null when the source is unreachable or returns no usable data.</summary>
        Task<WeatherResponse?> GetAtAsync(double latitude, double longitude, CancellationToken ct = default);
    }
}