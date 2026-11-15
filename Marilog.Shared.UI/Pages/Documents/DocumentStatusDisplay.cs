using Marilog.Contracts.DTOs.Responses;
using Marilog.Kernel.Enums;
using MudBlazor;

namespace Marilog.Shared.UI.Pages.Documents
{
    /// <summary>
    /// Status chip + due-date text shared by DocumentTable, DocumentTreeRow
    /// and SharedDocumentDetailPage, so they never drift apart again.
    /// </summary>
    public static class DocumentStatusDisplay
    {
        private const int DueSoonDays = 10;

        public static (string Text, string Icon, Color Color) Payment(DocumentResponse d)
        {
            if (d.Side == FinancialSide.None)
                return ("None", Icons.Material.Filled.RemoveCircle, Color.Default);

            return d.PaymentStatus switch
            {
                DocumentPaymentStatus.Paid => ("Paid", Icons.Material.Filled.CheckCircle, Color.Success),
                DocumentPaymentStatus.PartiallyPaid => ("Partial", Icons.Material.Filled.Timelapse, Color.Info),
                _ => ("Unpaid", Icons.Material.Filled.Schedule, Color.Warning)
            };
        }

        /// <summary>Null = nothing to show (no due date, non-financial, or already paid).</summary>
        public static (string Text, Color Color)? Due(DocumentResponse d)
        {
            if (d.DocDue is not DateOnly due
                || d.Side == FinancialSide.None
                || d.PaymentStatus == DocumentPaymentStatus.Paid)
                return null;

            var days = due.DayNumber - DateOnly.FromDateTime(DateTime.Today).DayNumber;

            if (days < 0) return ($"{Days(-days)} overdue", Color.Error);
            if (days == 0) return ("Due today", Color.Warning);
            if (days <= DueSoonDays) return ($"{Days(days)} left", Color.Warning);
            return ($"{Days(days)} left", Color.Success);
        }

        private static string Days(int n) => n == 1 ? "1 day" : $"{n} days";
    }
}