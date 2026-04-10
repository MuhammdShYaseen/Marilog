namespace Marilog.Contracts.DTOs.Reports.SwiftTransferReports
{
    public class MonthlyTransferSummary
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalUnallocated { get; set; }
    }
}