

using Marilog.Domain.Common;

namespace Marilog.Domain.Entities.SystemEntities
{
    public class DocumentAdjustment : Entity
    {
        public int DocumentId { get; private set; }
        public decimal Amount { get; private set; }        // موجب = زيادة، سالب = تخفيض
        public DateOnly AdjustmentDate { get; private set; }
        public string? Reason { get; private set; }

        private DocumentAdjustment() { }

        public static DocumentAdjustment Create(int documentId, decimal amount,
            DateOnly adjustmentDate, string? reason)
        {
            if (amount == 0) throw new ArgumentException("Adjustment amount cannot be zero.");

            return new DocumentAdjustment
            {
                DocumentId = documentId,
                Amount = amount,
                AdjustmentDate = adjustmentDate,
                Reason = reason
            };
        }

        public void Update(decimal amount, DateOnly adjustmentDate, string? reason)
        {
            if (amount == 0) throw new ArgumentException("Adjustment amount cannot be zero.");
            Amount = amount;
            AdjustmentDate = adjustmentDate;
            Reason = reason;
            Touch();
        }
    }
}
