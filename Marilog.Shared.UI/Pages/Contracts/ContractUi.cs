using MudBlazor;

namespace Marilog.Shared.UI.Pages.Contracts
{
    /// <summary>
    /// UI mirror of the ContractStatus rules in the domain — used to show/hide buttons only.
    /// The server still enforces every rule.
    /// </summary>
    public static class ContractUi
    {
        public static readonly string[] Statuses = ["Draft", "Active", "Suspended", "Expired", "Terminated"];

        public static DateOnly Today => DateOnly.FromDateTime(DateTime.Today);

        public static bool IsClosed(string status) => status is "Expired" or "Terminated";
        public static bool CanActivate(string status) => status is "Draft" or "Suspended";
        public static bool CanSuspend(string status) => status is "Active";
        public static bool CanTerminate(string status) => status is "Active" or "Suspended";
        public static bool CanDelete(string status) => status is "Draft";

        public static bool CanMarkExpired(string status, DateOnly? expiry)
            => !IsClosed(status) && expiry.HasValue && expiry.Value < Today;

        public static Color StatusColor(string status) => status switch
        {
            "Active" => Color.Success,
            "Suspended" => Color.Warning,
            "Terminated" => Color.Error,
            "Expired" => Color.Dark,
            _ => Color.Default
        };

        /// <summary>Overdue (past expiry, still open) → Error; within 30 days → Warning.</summary>
        public static Color ExpiryColor(string status, DateOnly? expiry)
        {
            if (!expiry.HasValue || IsClosed(status))
                return Color.Inherit;

            if (expiry.Value < Today)
                return Color.Error;

            return expiry.Value <= Today.AddDays(30) ? Color.Warning : Color.Inherit;
        }
    }
}
