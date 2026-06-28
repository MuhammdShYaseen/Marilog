

using Marilog.Kernel.Enums;

namespace Marilog.Contracts.DTOs.Requests.DocumentDTOs
{
    public class UpdatePaymentRequest
    {
        public int? SwiftTransferId { get; set; }
        public decimal PaidAmount { get; set; }
        public DateOnly PaymentDate { get; set; }
        public PaymentMethod Method { get; set; }
    }
}
