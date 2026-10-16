using System.Text.RegularExpressions;
using Marilog.Kernel.Enums;
using MudBlazor;

namespace Marilog.Shared.UI.Components.Certificates
{
    public static class CertificateUi
    {
        public static Color GetColor(CertificateExpiryStatus status) => status switch
        {
            CertificateExpiryStatus.Expired => Color.Error,
            CertificateExpiryStatus.ExpiringSoon => Color.Warning,
            CertificateExpiryStatus.Valid => Color.Success,
            _ => Color.Default
        };

        public static string GetCssVar(CertificateExpiryStatus status) => status switch
        {
            CertificateExpiryStatus.Expired => "var(--mud-palette-error)",
            CertificateExpiryStatus.ExpiringSoon => "var(--mud-palette-warning)",
            CertificateExpiryStatus.Valid => "var(--mud-palette-success)",
            _ => "var(--mud-palette-text-secondary)"
        };

        public static string GetLabel(CertificateExpiryStatus status) => status switch
        {
            CertificateExpiryStatus.Expired => "Expired",
            CertificateExpiryStatus.ExpiringSoon => "Expiring soon",
            CertificateExpiryStatus.Valid => "Valid",
            _ => "No expiry"
        };

        public static int? DaysLeft(DateOnly? expiryDate, DateOnly today)
            => expiryDate is null ? null : expiryDate.Value.DayNumber - today.DayNumber;
        public static string FormatDaysLeft(int days)
        {
            if (days == 0) return "Expires today";
            var span = FormatSpan(Math.Abs(days));
            return days < 0 ? $"{span} overdue" : $"{span} left";
        }

        private static string FormatSpan(int days) => days switch
        {
            <= 90 => $"{days} d",
            <= 730 => $"{days / 30} mo",
            _ => $"{days / 365} y"
        };
        public static string TypeLabel(VesselCertificateType? type)
            => type is null ? "—" : Humanize(type.Value.ToString());

        private static string Humanize(string name)
            => Regex.Replace(name.Replace('_', ' '), "(?<=[a-z])(?=[A-Z])", " ");
    }
}