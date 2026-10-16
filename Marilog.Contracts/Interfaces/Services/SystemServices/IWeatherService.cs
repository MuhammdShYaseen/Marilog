using Marilog.Contracts.DTOs.Responses;

namespace Marilog.Contracts.Interfaces.Services.SystemServices
{
    public interface IWeatherService
    {
        Task<WeatherResponse?> GetAtAsync(double latitude, double longitude, CancellationToken ct = default);
    }
}