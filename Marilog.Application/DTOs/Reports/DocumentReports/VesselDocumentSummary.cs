namespace Marilog.Application.DTOs.Reports.DocumentReports
{
    internal class VesselDocumentSummary
    {
        public int VesselId { get; set; }
        public string? VesselName { get; set; }
        public decimal TotalValue { get; set; }
        public decimal? TotalPaid { get; set; }
        public decimal TotalRemain { get; set; }
        public int Count { get; set; }
    }
}