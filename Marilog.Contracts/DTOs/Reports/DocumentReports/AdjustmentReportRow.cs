using Marilog.Kernel.Enums;

namespace Marilog.Contracts.DTOs.Reports.DocumentReports
{
    public class AdjustmentReportRow
    {
        public int AdjustmentId { get; set; }
        public DateOnly AdjustmentDate { get; set; }
        public decimal Amount { get; set; }
        public string? Reason { get; set; }

        public int DocumentId { get; set; }
        public string DocNumber { get; set; } = string.Empty;
        public DateOnly DocDate { get; set; }
        public string? DocTypeName { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;

        public string? SupplierName { get; set; }
        public string? BuyerName { get; set; }
        public string? VesselName { get; set; }
        public FinancialSide Side { get; set; }
    }
}