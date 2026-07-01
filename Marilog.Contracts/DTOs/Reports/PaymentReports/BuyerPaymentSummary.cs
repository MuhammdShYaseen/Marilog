

namespace Marilog.Contracts.DTOs.Reports.PaymentReports
{
    public class BuyerPaymentSummary
    {
        public int BuyerId { get; set; }
        public string? BuyerName { get; set; }
        public decimal TotalReceivedBase { get; set; }
        public int Count { get; set; }
    }
}
