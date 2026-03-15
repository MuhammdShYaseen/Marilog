namespace Marilog.Domain.Entities
{
    public class Payment
    {
        public int Id { get; private set; }
        public int DocumentId { get; private set; }
        public int SwiftTransferId { get; private set; }
        public SwiftTransfer SwiftTransfer { get; private set; } = null!;
        public decimal PaidAmount { get; private set; }
        public DateOnly PaymentDate { get; private set; }

        private Payment() { }
        internal static Payment Create(int documentId, int swiftTransferId,
            decimal paidAmount, DateOnly paymentDate)
        {
            if (swiftTransferId <= 0) throw new ArgumentException("Invalid SwiftTransferID.");
            if (paidAmount <= 0) throw new ArgumentException("PaidAmount must be positive.");

            return new Payment
            {
                DocumentId = documentId,
                SwiftTransferId = swiftTransferId,
                PaidAmount = paidAmount,
                PaymentDate = paymentDate
            };
        }
    }
}