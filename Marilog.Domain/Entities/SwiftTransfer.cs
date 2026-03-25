using Marilog.Domain.Common;

namespace Marilog.Domain.Entities
{
        public class SwiftTransfer : Entity
        {
            public string SwiftReference { get; private set; } = null!;
            public DateOnly TransactionDate { get; private set; }
            public int CurrencyId { get; private set; }
            public Currency Currency { get; private set; } = null!;
            public decimal Amount { get; private set; }
            public int? SenderCompanyId { get; private set; }
            public Company? SenderCompany { get; private set; }
            public int? ReceiverCompanyId { get; private set; }
            public Company? ReceiverCompany { get; private set; }
            public string? SenderBank { get; private set; }
            public string? ReceiverBank { get; private set; }
            public string? PaymentReference { get; private set; }
            public string? RawMessage { get; private set; }

            private readonly List<Payment> _payments = new();
            public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();

            private SwiftTransfer() { }
            public static SwiftTransfer Create(string swiftReference, DateOnly transactionDate,
                int currencyId, decimal amount, int? senderCompanyId = null,
                int? receiverCompanyId = null, string? senderBank = null,
                string? receiverBank = null, string? paymentReference = null,
                string? rawMessage = null)
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(swiftReference);
                if (amount <= 0) throw new ArgumentException("Amount must be positive.");

                return new SwiftTransfer
                {
                    SwiftReference = swiftReference,
                    TransactionDate = transactionDate,
                    CurrencyId = currencyId,
                    Amount = amount,
                    SenderCompanyId = senderCompanyId,
                    ReceiverCompanyId = receiverCompanyId,
                    SenderBank = senderBank,
                    ReceiverBank = receiverBank,
                    PaymentReference = paymentReference,
                    RawMessage = rawMessage
                };
            }

            public void Update(int currencyId, decimal amount, string? senderBank,
                string? receiverBank, string? paymentReference, string? rawMessage)
            {
                if (amount <= 0) throw new ArgumentException("Amount must be positive.");

                CurrencyId = currencyId;
                Amount = amount;
                SenderBank = senderBank;
                ReceiverBank = receiverBank;
                PaymentReference = paymentReference;
                RawMessage = rawMessage;
                Touch();
            }

        // ── Computed ────────────────────────────────────────────────────────────
        public decimal AllocatedAmount => _payments.Sum(p => p.PaidAmount);
        public decimal UnallocatedAmount => Amount - AllocatedAmount;
        public bool IsFullyAllocated => UnallocatedAmount <= 0;
    }
}
