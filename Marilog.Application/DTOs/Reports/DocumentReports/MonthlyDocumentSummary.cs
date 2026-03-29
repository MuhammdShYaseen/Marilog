namespace Marilog.Application.DTOs.Reports.DocumentReports
{
    internal class MonthlyDocumentSummary
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal? TotalValue { get; set; }
        public decimal? TotalPaid { get; set; }
        public decimal? TotalRemain { get; set; }
        public int Count { get; set; }
    }
}