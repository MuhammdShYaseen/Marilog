namespace Marilog.Application.DTOs.Reports.DocumentReports
{
    internal class SupplierDocumentSummary
    {
        public int SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public decimal? TotalValue { get; set; }
        public decimal? TotalPaid { get; set; }
        public decimal? TotalRemain { get; set; }
        public int Count { get; set; }
    }
}