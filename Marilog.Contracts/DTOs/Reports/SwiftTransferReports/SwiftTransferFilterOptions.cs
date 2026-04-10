namespace Marilog.Contracts.DTOs.Reports.SwiftTransferReports
{
    public class SwiftTransferFilterOptions
    {
        public int? Id { get; set; }
        public string? Reference { get; set; }
        public int? SenderCompanyId { get; set; }
        public int? ReceiverCompanyId { get; set; }
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
        public bool OnlyUnallocated { get; set; } = false;
        public bool IncludePayments { get; set; } = false;
    }
}
