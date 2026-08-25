

namespace Marilog.Contracts.DTOs.Requests.CreditBalance
{
    public class CreateCreditBalanceRequest
    {
        public int CurrencyId { get; set; }
        public int PaymentId { get; set; }
        public decimal Amount { get; set; }
        public int? SenderCompanyId { get; set; }
        public int? ReceiverCompanyId { get; set; }
    }
}
