using Marilog.Kernel.Enums;

namespace Marilog.Contracts.DTOs.Responses
{
    public class PaymentResponse
    {
        public int Id { get;  set; }
        public int DocumentId { get;  set; }
        public int? SwiftTransferId { get;  set; }
        public SwiftTransferResponse? SwiftTransfer { get;  set; } = new();
        public decimal PaidAmount { get;  set; }
        public DateOnly PaymentDate { get;  set; }
        public PaymentMethod PaymentMethod { get; set; }
        public bool IsActive {  get; set; }
    }
}