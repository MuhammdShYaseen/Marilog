namespace Marilog.Presentation.DTOs.DocumentDTOs
{
    public class AddPaymentRequest
    {
        public int SwiftTransferId { get; set; }
        public decimal PaidAmount { get; set; }
        public DateOnly PaymentDate { get; set; }
    }
}
