
using Marilog.Contracts.DTOs.Responses;

namespace Marilog.Contracts.DTOs.Reports.DocumentReports
{
    public class DocumentReport
    {
        public IReadOnlyList<DocumentResponse> Documents { get; set; } = Array.Empty<DocumentResponse>();
        public decimal TotalValue { get; set; }
        public int Count { get; set; }
        public string BaseCurrencyCode { get; set; } = null!;
        public IReadOnlyList<MonthlyTotal> MonthlyTotals { get; set; } = Array.Empty<MonthlyTotal>();
        public IReadOnlyList<YearlyTotal> YearlyTotals { get; set; } = Array.Empty<YearlyTotal>();
        public decimal TotalPaid { get; set; }
        public decimal TotalRemaining { get; set; }
        public IReadOnlyList<MonthlyDocumentSummary> MonthlySummary { get; init; } = [];
        public IReadOnlyList<SupplierDocumentSummary> SupplierSummary { get; init; } = [];
        public IReadOnlyList<VesselDocumentSummary> VesselSummary { get; init; } = [];
        public IReadOnlyList<FinancelSideDocumentSummary> RevenueSideSummary { get; init; } = [];
        public IReadOnlyList<FinancelSideDocumentSummary> ExpenseSideSummary { get; init; } = [];
        public IReadOnlyList<FinancelSideDocumentSummary> NoneSideSummary { get; init; } = [];
        public List<VoyageDocumentSummary> VoyageSummary { get; init; } = [];
        public decimal NetPosition { get; set; }
       
    }
}
