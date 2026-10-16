using System.Globalization;
using MudBlazor;

namespace Marilog.Shared.UI.Components.Weather
{
    public static class WeatherUi
    {
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        private static readonly string[] Points =
            ["N", "NNE", "NE", "ENE", "E", "ESE", "SE", "SSE", "S", "SSW", "SW", "WSW", "W", "WNW", "NW", "NNW"];

        public static string Compass(double? deg) =>
            deg is double d ? Points[(int)Math.Round((((d % 360) + 360) % 360) / 22.5) % 16] : "—";

        public static string BeaufortName(int? force) => force switch
        {
            0 => "Calm",
            1 => "Light air",
            2 => "Light breeze",
            3 => "Gentle breeze",
            4 => "Moderate breeze",
            5 => "Fresh breeze",
            6 => "Strong breeze",
            7 => "Near gale",
            8 => "Gale",
            9 => "Strong gale",
            10 => "Storm",
            11 => "Violent storm",
            12 => "Hurricane force",
            _ => "—"
        };

        public static Color BeaufortColor(int? force) => force switch
        {
            >= 8 => Color.Error,
            >= 6 => Color.Warning,
            _ => Color.Default
        };

        // WMO / Douglas sea state by significant wave height
        public static (int Code, string Name) SeaState(double waveHeightM) => waveHeightM switch
        {
            <= 0 => (0, "Calm (glassy)"),
            <= 0.1 => (1, "Calm (rippled)"),
            <= 0.5 => (2, "Smooth"),
            <= 1.25 => (3, "Slight"),
            <= 2.5 => (4, "Moderate"),
            <= 4 => (5, "Rough"),
            <= 6 => (6, "Very rough"),
            <= 9 => (7, "High"),
            <= 14 => (8, "Very high"),
            _ => (9, "Phenomenal")
        };

        public static Color WaveColor(double? heightM) => heightM switch
        {
            >= 4 => Color.Error,
            >= 2.5 => Color.Warning,
            _ => Color.Default
        };

        public static string Number(double? value, string format = "0.#") =>
            value?.ToString(format, Inv) ?? "—";

        public static string Visibility(double? meters) => meters switch
        {
            null => "—",
            >= 1000 => $"{(meters.Value / 1000).ToString("0.#", Inv)} km",
            _ => $"{meters.Value.ToString("0", Inv)} m"
        };

        public static Color VisibilityColor(double? meters) => meters switch
        {
            < 1000 => Color.Error,
            < 5000 => Color.Warning,
            _ => Color.Default
        };
    }
}