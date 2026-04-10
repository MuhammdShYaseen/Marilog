namespace Marilog.Contracts.DTOs.Requests.SwiftTransferDTOs
{
    public class CreateSwiftTransferRequest
    {
        public string SwiftReference { get; set; } = null!;
        public DateOnly TransactionDate { get; set; }
        public int CurrencyId { get; set; }
        public decimal Amount { get; set; }
        public int? SenderCompanyId { get; set; } = null;
        public int? ReceiverCompanyId { get; set; } = null;
        public string? SenderBank { get; set; } = null;
        public string? ReceiverBank { get; set; } = null;
        public string? PaymentReference { get; set; } = null;
        public string? RawMessage { get; set; } = null;
    }
}
