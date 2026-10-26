using Marilog.Domain.Common;
using Marilog.Kernel.Enums;

namespace Marilog.Domain.Entities.SystemEntities
{
    public class SwiftBankFee : Entity
    {
        public int SwiftTransferId { get; private set; }
        public SwiftFeeStage Stage { get; private set; }
        public int? BankId { get; private set; }
        public Bank? Bank { get; private set; }
        public decimal Amount { get; private set; }
        public bool IsDeducted { get; private set; }
        public DateOnly FeeDate { get; private set; }
        public string? Description { get; private set; }

        private SwiftBankFee() { } // EF

        internal static SwiftBankFee Create(SwiftFeeStage stage, decimal amount, bool isDeducted,
            DateOnly feeDate, int? bankId, string? description)
        {
            Validate(stage, amount);

            return new SwiftBankFee
            {
                Stage = stage,
                Amount = amount,
                IsDeducted = isDeducted,
                FeeDate = feeDate,
                BankId = bankId,
                Description = Normalize(description)
            };
        }

        internal void Update(SwiftFeeStage stage, decimal amount, bool isDeducted,
            DateOnly feeDate, int? bankId, string? description)
        {
            Validate(stage, amount);

            Stage = stage;
            Amount = amount;
            IsDeducted = isDeducted;
            FeeDate = feeDate;
            BankId = bankId;
            Description = Normalize(description);
            Touch();
        }

        private static void Validate(SwiftFeeStage stage, decimal amount)
        {
            if (!Enum.IsDefined(stage))
                throw new ArgumentException("Invalid fee stage.", nameof(stage));

            if (amount <= 0)
                throw new ArgumentException("Fee amount must be positive.", nameof(amount));
        }

        private static string? Normalize(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}