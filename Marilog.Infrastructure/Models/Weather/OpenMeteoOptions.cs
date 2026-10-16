

namespace Marilog.Infrastructure.Models.Weather
{
    public sealed class OpenMeteoOptions
    {
        public const string SectionName = "OpenMeteo";

        // Free tier. Commercial plan: https://customer-api.open-meteo.com / https://customer-marine-api.open-meteo.com + ApiKey
        public string ForecastBaseUrl { get; set; } = "https://api.open-meteo.com";
        public string MarineBaseUrl { get; set; } = "https://marine-api.open-meteo.com";
        public string? ApiKey { get; set; }
    }
}
