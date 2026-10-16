using System.Globalization;
using Marilog.Contracts.DTOs.Responses;

namespace Marilog.Shared.UI.Pages.Vessels
{
    public static class VesselPositionUi
    {
        public const double PositionPrecisionDegrees = 1.0;

        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        public static bool IsStationary(VesselPositionInfo p) =>
            p.Speed < 0.5 ||
            (p.NavigationStatus?.Contains("anchor", StringComparison.OrdinalIgnoreCase) ?? false) ||
            (p.NavigationStatus?.Contains("moored", StringComparison.OrdinalIgnoreCase) ?? false);

        public static string StatusText(VesselPositionInfo p) =>
            !string.IsNullOrWhiteSpace(p.NavigationStatus) ? p.NavigationStatus!
            : IsStationary(p) ? "Stationary" : "Under way";

        public static DateTime AsUtc(DateTime d) => d.Kind switch
        {
            DateTimeKind.Utc => d,
            DateTimeKind.Local => d.ToUniversalTime(),
            _ => DateTime.SpecifyKind(d, DateTimeKind.Utc)
        };

        public static string FormatUtc(DateTime d) => AsUtc(d).ToString("dd MMM HH:mm", Inv) + " UTC";

        public static string Ago(DateTime d)
        {
            var span = DateTime.UtcNow - AsUtc(d);
            if (span.TotalMinutes < 1) return "just now";
            if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes} min ago";
            if (span.TotalHours < 48) return $"{(int)span.TotalHours} h ago";
            return $"{(int)span.TotalDays} days ago";
        }

        public static bool IsStale(DateTime d) => DateTime.UtcNow - AsUtc(d) > TimeSpan.FromHours(24);

        public static (string Value, string Unit) Elapsed(DateTime fromUtc)
        {
            var span = DateTime.UtcNow - AsUtc(fromUtc);
            return span.TotalHours < 48
                ? (((int)Math.Max(0, span.TotalHours)).ToString(Inv), "h")
                : (((int)span.TotalDays).ToString(Inv), "days");
        }

        public static string FormatCoordinates(double lat, double lon) =>
            $"{Dms(lat, 'N', 'S', 2)}  {Dms(lon, 'E', 'W', 3)}";

        private static string Dms(double value, char pos, char neg, int pad)
        {
            var abs = Math.Abs(value);
            var deg = (int)Math.Floor(abs);
            var min = (int)Math.Round((abs - deg) * 60);
            if (min == 60) { deg++; min = 0; }
            return $"{deg.ToString(Inv).PadLeft(pad, '0')}°{min:00}′{(value >= 0 ? pos : neg)}";
        }

        public static string PrecisionNote(double lat)
        {
            var nmNs = (int)Math.Round(60 * PositionPrecisionDegrees);
            var nmEw = (int)Math.Round(60 * PositionPrecisionDegrees * Math.Cos(lat * Math.PI / 180));
            return $"The source rounds the position to whole degrees, so the map shows an area of about " +
                   $"{nmNs} × {nmEw} nm, not an exact fix. Don't use it for arrival or anchorage timing.";
        }

        public static IReadOnlyList<string> CompareWithRecord(VesselResponse vessel, VesselPositionResponse position)
        {
            var differences = new List<string>();

            if (!string.IsNullOrWhiteSpace(position.Name) &&
                !string.Equals(vessel.Name.Trim(), position.Name.Trim(), StringComparison.OrdinalIgnoreCase))
                differences.Add($"Name — Marilog: {vessel.Name} · VesselFinder: {position.Name}");

            if (!string.IsNullOrWhiteSpace(position.Flag) && !string.IsNullOrWhiteSpace(vessel.FlagCountryName) &&
                !string.Equals(vessel.FlagCountryName.Trim(), position.Flag.Trim(), StringComparison.OrdinalIgnoreCase))
                differences.Add($"Flag — Marilog: {vessel.FlagCountryName} · VesselFinder: {position.Flag}");

            if (position.Dimensions.GrossTonnage is int vfGt && vessel.GrossTonnage is decimal gt &&
                Math.Abs(gt - vfGt) > Math.Max(1m, vfGt * 0.01m))
                differences.Add($"Gross tonnage — Marilog: {gt.ToString("N0", Inv)} · VesselFinder: {vfGt.ToString("N0", Inv)}");

            return differences;
        }
    }
}