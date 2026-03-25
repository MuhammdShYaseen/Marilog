using Marilog.Domain.Entities;

namespace Marilog.Application.DTOs
{
    public class PaymentResponse
    {
        public int Id { get;  set; }
        public int DocumentId { get;  set; }
        public int SwiftTransferId { get;  set; }
        public SwiftTransferResponse SwiftTransfer { get;  set; } = new();
        public decimal PaidAmount { get;  set; }
        public DateOnly PaymentDate { get;  set; }
    }
}