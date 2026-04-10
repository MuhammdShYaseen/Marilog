namespace Marilog.Contracts.DTOs.Reports.SwiftTransferReports
{
    public class CompanyTransferSummary
    {
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = "";
        public decimal TotalAmount { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalUnallocated { get; set; }
        public int TransfersCount { get; set; }
    }
}