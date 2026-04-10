using Marilog.Contracts.DTOs.Responses;

namespace Marilog.Contracts.DTOs.Reports.SwiftTransferReports
{
    public class SwiftTransferReport
    {
        public IReadOnlyList<SwiftTransferResponse> Transfers { get; set; } = Array.Empty<SwiftTransferResponse>();

        public decimal TotalAmount { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalUnallocated { get; set; }

        public IReadOnlyList<MonthlyTransferSummary> MonthlySummary { get; set; } = Array.Empty<MonthlyTransferSummary>();
        public IReadOnlyList<CompanyTransferSummary> SenderSummary { get; set; } = Array.Empty<CompanyTransferSummary>();
        public IReadOnlyList<CompanyTransferSummary> ReceiverSummary { get; set; } = Array.Empty<CompanyTransferSummary>();
    }

}
