namespace Marilog.Contracts.DTOs.Reports.DocumentReports
{
    public class VesselDocumentSummary
    {
        public int VesselId { get; set; }
        public string? VesselName { get; set; }
        public decimal TotalValue { get; set; }
        public decimal? TotalPaid { get; set; }
        public decimal TotalRemain { get; set; }
        public decimal Revenue { get; set; }
        public decimal Expense { get; set; }
        public decimal NetPosition { get; set; }
        public int Count { get; set; }
    }
}