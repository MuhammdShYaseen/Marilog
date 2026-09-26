using Marilog.Domain.Common;
using Marilog.Kernel.Enums;

namespace Marilog.Domain.Entities.SystemEntities
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
            public int? SenderBankId { get; private set; }
            public Bank? SenderBank { get; private set; }
            public int? ReceiverBankId { get; private set; }
            public Bank? ReceiverBank { get; private set; }
            public string? PaymentReference { get; private set; }
            public string? RawMessage { get; private set; }


        //====Status=================================================================
            public SwiftTransferStatus Status { get; private set; } = SwiftTransferStatus.Pending;
            public DateOnly? ReceivedDate { get; private set; }
            public string? CancellationReason { get; private set; }
        //===========================================================================
            private readonly List<Payment> _payments = new();
            public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();

            private SwiftTransfer() { }
            public static SwiftTransfer Create(string swiftReference, DateOnly transactionDate,
                int currencyId, decimal amount, int? senderCompanyId = null,
                int? receiverCompanyId = null, int? senderBankId = null,
                int? receiverBankId = null, string? paymentReference = null,
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
                    SenderBankId = senderBankId,
                    ReceiverBankId = receiverBankId,
                    PaymentReference = paymentReference,
                    RawMessage = rawMessage
                };
            }

            public void Update(int currencyId, decimal amount, int? senderBankId,
                int? receiverBankId, string? paymentReference, string? rawMessage)
            {
                if (amount <= 0) throw new ArgumentException("Amount must be positive.");

                CurrencyId = currencyId;
                Amount = amount;
                SenderBankId = senderBankId;
                ReceiverBankId = receiverBankId;
                PaymentReference = paymentReference;
                RawMessage = rawMessage;
                Touch();
            }

        public void SetRawMessage(string swiftRowMessge)
        {
            if (string.IsNullOrWhiteSpace(RawMessage)) 
                throw new ArgumentNullException("RawMessage is empty");

            RawMessage = swiftRowMessge;
        }


        public void MarkReceived(DateOnly receivedDate)
        {
            if (Status == SwiftTransferStatus.Cancelled)
                throw new InvalidOperationException("Cannot confirm a cancelled SWIFT transfer.");

            if (receivedDate < TransactionDate)
                throw new ArgumentException("Received date cannot be before the transaction date.");

            Status = SwiftTransferStatus.Received;
            ReceivedDate = receivedDate;
            Touch();
        }

        public void Cancel(string reason)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(reason);

            if (Status == SwiftTransferStatus.Cancelled)
                throw new InvalidOperationException("SWIFT transfer is already cancelled.");

            if (_payments.Count > 0)
                throw new InvalidOperationException("Remove the allocated payments before cancelling this SWIFT transfer.");

            Status = SwiftTransferStatus.Cancelled;
            CancellationReason = reason.Trim();
            Touch();
        }

        // ── Computed ────────────────────────────────────────────────────────────
        public decimal AllocatedAmount => _payments.Sum(p => p.PaidAmount);
        public decimal UnallocatedAmount => Amount - AllocatedAmount;
        public bool IsFullyAllocated => UnallocatedAmount <= 0;
    }
}
