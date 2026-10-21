using System.Globalization;
using Marilog.Kernel.Enums;
using MudBlazor;

namespace Marilog.Shared.UI.Pages.Audit
{
    internal static class AuditDisplay
    {
        // Invariant: an Arabic culture could switch the calendar or digits
        public static string Local(DateTime utc) =>
            utc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

        public static Color OperationColor(AuditOperation operation) => operation switch
        {
            AuditOperation.Create => Color.Success,
            AuditOperation.Update => Color.Info,
            AuditOperation.Delete => Color.Error,
            AuditOperation.Purge => Color.Warning,
            _ => Color.Default
        };
    }
}