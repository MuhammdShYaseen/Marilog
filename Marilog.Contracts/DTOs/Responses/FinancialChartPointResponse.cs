

namespace Marilog.Contracts.DTOs.Responses
{
    public class FinancialChartPointResponse
    {
        public DateTime PeriodStart { get; set; }
        public string? PeriodLabel { get; set; }
        public decimal Revenue { get; set; }
        public decimal Expense { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
    }
}
