namespace Marilog.Contracts.DTOs.Reports.DocumentReports
{
    public class MonthlyTotal
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal TotalValue { get; set; }
        public int Count { get; set; }
    }
}