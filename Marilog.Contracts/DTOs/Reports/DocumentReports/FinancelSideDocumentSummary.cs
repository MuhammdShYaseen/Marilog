

namespace Marilog.Contracts.DTOs.Reports.DocumentReports
{
    public class FinancelSideDocumentSummary
    {
        public string Side { get; set; } = "None";
        public decimal TotalValue { get; set; }
        public decimal? TotalPaid { get; set; }
        public decimal TotalRemain { get; set; }
        public int Count { get; set; }
    }
}
