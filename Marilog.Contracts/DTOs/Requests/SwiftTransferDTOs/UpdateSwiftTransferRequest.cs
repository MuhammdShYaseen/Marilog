namespace Marilog.Contracts.DTOs.Requests.SwiftTransferDTOs
{
    public class UpdateSwiftTransferRequest
    {
        public int CurrencyId { get; set; }
        public decimal Amount { get; set; }
        public int? SenderBankId { get; set; } = null;
        public int? ReceiverBankId { get; set; } = null;
        public string? PaymentReference { get; set; } = null;
        public string? RawMessage { get; set; } = null;
    }
}
