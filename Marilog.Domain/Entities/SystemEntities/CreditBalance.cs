

using Marilog.Domain.Common;

namespace Marilog.Domain.Entities.SystemEntities
{
    public class CreditBalance : Entity
    {
        public int PaymentId { get; private set; }
        public int CurrencyId { get; private set; }
        public Currency Currency { get; private set; } = null!;
        public decimal Amount { get; private set; }
        public int? SenderCompanyId { get; private set; }
        public Company? SenderCompany { get; private set; }
        public int? ReceiverCompanyId { get; private set; }
        public Company? ReceiverCompany { get; private set; }
        private readonly List<Payment> _payments = new();
        public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();
        private CreditBalance()
        {
        }

        public static CreditBalance Create(int paymentId, int currencyId, decimal amount, int? senderCompanyId, int? receiverCompanyId)
        {
            if (paymentId <= 0)
                throw new ArgumentException("PaymentId must be greater than zero.");

            if (currencyId <= 0)
                throw new ArgumentException("CurrencyId must be greater than zero.");

            if (amount <= 0)
                throw new ArgumentException("Amount must be positive.");

            if (senderCompanyId.HasValue && senderCompanyId.Value <= 0)
                throw new ArgumentException("SenderCompanyId must be greater than zero.");

            if (receiverCompanyId.HasValue && receiverCompanyId.Value <= 0)
                throw new ArgumentException("ReceiverCompanyId must be greater than zero.");

            return new CreditBalance
            {
                PaymentId = paymentId,
                CurrencyId = currencyId,
                Amount = amount,
                SenderCompanyId = senderCompanyId,
                ReceiverCompanyId = receiverCompanyId
            };
        }

        public void Update(int currencyId, decimal amount, int? senderCompanyId, int? receiverCompanyId)
        {
            if (currencyId <= 0)
                throw new ArgumentException("CurrencyId must be greater than zero.");

            if (amount <= 0)
                throw new ArgumentException("Amount must be positive.");

            if (amount < AllocatedAmount)
                throw new InvalidOperationException(
                    "Amount cannot be less than the already allocated amount.");

            CurrencyId = currencyId;
            Amount = amount;
            SenderCompanyId = senderCompanyId;
            ReceiverCompanyId = receiverCompanyId;

            Touch();
        }

        // ── Computed ────────────────────────────────────────────────────────────

        public decimal AllocatedAmount =>
            _payments.Sum(p => p.PaidAmount);

        public decimal UnallocatedAmount =>
            Amount - AllocatedAmount;

        public bool IsFullyAllocated =>
            UnallocatedAmount <= 0;

        public bool HasAvailableBalance =>
            UnallocatedAmount > 0;
    }
}
