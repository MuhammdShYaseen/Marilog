

namespace Marilog.Contracts.DTOs.Responses
{
    public class CreditBalanceResponse
    {
        public int Id { get; set; }
        public int PaymentId { get; set; }
        public int CurrencyId { get; set; }
        public string? CurrencyCode { get; set; }
        public decimal Amount { get; set; }
        public decimal AllocatedAmount { get; set; }
        public decimal UnallocatedAmount { get; set; }
        public bool IsFullyAllocated { get; set; }
        public int? SenderCompanyId { get; set; }
        public string? SenderCompanyName { get; set; }
        public int? ReceiverCompanyId { get; set; }
        public string? ReceiverCompanyName { get; set; }
    }
}
