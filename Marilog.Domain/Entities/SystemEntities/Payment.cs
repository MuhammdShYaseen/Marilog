using Marilog.Domain.Common;
using Marilog.Kernel.Enums;

namespace Marilog.Domain.Entities.SystemEntities
{
    public class Payment : Entity
    {
        //public int Id { get; private set; }
        public int DocumentId { get; private set; }
        public int? SwiftTransferId { get; private set; }
        public SwiftTransfer SwiftTransfer { get; private set; } = null!;
        public decimal PaidAmount { get; private set; }
        public DateOnly PaymentDate { get; private set; }
        public PaymentMethod PaymentMethod { get; private set; }
        private Payment() { }
        internal static Payment Create(int documentId, PaymentMethod method, int? swiftTransferId,
            decimal paidAmount, DateOnly paymentDate)
        {
            if (paidAmount <= 0) throw new ArgumentException("PaidAmount must be positive.");

            return new Payment
            {
                DocumentId = documentId,
                SwiftTransferId = swiftTransferId,
                PaidAmount = paidAmount,
                PaymentDate = paymentDate,
                PaymentMethod = method
            };
        }

        internal void Update(int? swiftTransferId, PaymentMethod method, decimal paidAmount, DateOnly paymentDate)
        {
            

            if (paidAmount <= 0)
                throw new ArgumentException("PaidAmount must be positive.");

            SwiftTransferId = swiftTransferId;
            PaidAmount = paidAmount;
            PaymentDate = paymentDate;
            PaymentMethod = method;
        }


    }
}