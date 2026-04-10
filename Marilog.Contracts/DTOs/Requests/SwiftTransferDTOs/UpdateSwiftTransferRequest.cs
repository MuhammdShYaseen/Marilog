namespace Marilog.Contracts.DTOs.Requests.SwiftTransferDTOs
{
    public class UpdateSwiftTransferRequest
    {
        public int CurrencyId { get; set; }
        public decimal Amount { get; set; }
        public string? SenderBank { get; set; } = null;
        public string? ReceiverBank { get; set; } = null;
        public string? PaymentReference { get; set; } = null;
        public string? RawMessage { get; set; } = null;
    }
}
