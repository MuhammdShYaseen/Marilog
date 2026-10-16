namespace Marilog.Contracts.DTOs.Responses
{
    public sealed record WeatherResponse(
        double Latitude,                // grid cell the model actually used (may differ slightly from the request)
        double Longitude,
        DateTime? ModelTimeUtc,         // the time the "current" values refer to
        CurrentWeather Current,
        CurrentSeaState? Sea,           // null when the marine model has no data for this point
        IReadOnlyList<DailyWeather> Daily);

    public sealed record CurrentWeather(
        double? TemperatureC,
        double? HumidityPercent,
        double? PressureHpa,            // mean sea level
        double? VisibilityM,
        int? WeatherCode,               // WMO code
        string? Description,
        double? WindSpeedKn,
        double? WindDirectionDeg,       // direction the wind blows FROM
        double? WindGustsKn,
        int? Beaufort);

    public sealed record CurrentSeaState(
        double? WaveHeightM,
        double? WaveDirectionDeg,       // direction the waves come FROM
        double? WavePeriodS,
        double? SwellHeightM,
        double? SwellDirectionDeg,
        double? SwellPeriodS,
        double? WindWaveHeightM,
        double? SeaSurfaceTemperatureC,
        double? CurrentSpeedKn,
        double? CurrentDirectionDeg);   // as reported by the source

    public sealed record DailyWeather(
        DateOnly Date,                  // GMT day
        int? WeatherCode,
        string? Description,
        double? TemperatureMinC,
        double? TemperatureMaxC,
        double? PrecipitationMm,
        double? WindSpeedMaxKn,
        double? WindGustsMaxKn,
        double? WindDirectionDominantDeg,
        int? BeaufortMax,
        double? WaveHeightMaxM,
        double? SwellHeightMaxM,
        double? WaveDirectionDominantDeg);
}