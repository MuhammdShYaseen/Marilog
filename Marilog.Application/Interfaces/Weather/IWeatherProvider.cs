using Marilog.Contracts.DTOs.Responses;

namespace Marilog.Application.Interfaces.Weather
{
    public interface IWeatherProvider
    {
        /// <summary>Returns null when the source is unreachable or returns no usable data.</summary>
        Task<WeatherResponse?> GetAtAsync(double latitude, double longitude, CancellationToken ct = default);


        /// <summary>Current wind at each point, in the same order. Returns an empty list on failure.</summary>
        Task<IReadOnlyList<WindFieldPoint>> GetCurrentWindAsync(
            IReadOnlyList<(double Latitude, double Longitude)> points, CancellationToken ct = default);
    }
}