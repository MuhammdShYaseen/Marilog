

namespace Marilog.Domain.ValueObjects.Contract
{
    public class ContractAmendment
    {
        // Identity داخل Aggregate
        public int AmendmentNumber { get; private set; }

        // وصف التعديل
        public string Description { get; private set; } = null!;

        // التاريخ الذي يبدأ فيه التعديل بالفعالية
        public DateOnly EffectiveDate { get; private set; }

        // من قام بالتعديل
        public string ChangedBy { get; private set; } = null!;

        // طابع زمني للتسجيل (Audit)
        public DateTime RecordedAtUtc { get; private set; }

        // EF Core constructor
        private ContractAmendment() { }

        // Constructor
        internal ContractAmendment(
            string description,
            DateOnly effectiveDate,
            string changedBy,
            DateTime recordedAtUtc)
        {

            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Description is required.");

            if (string.IsNullOrWhiteSpace(changedBy))
                throw new ArgumentException("ChangedBy is required.");

            Description = description;
            EffectiveDate = effectiveDate;
            ChangedBy = changedBy;
            RecordedAtUtc = recordedAtUtc;
        }

        // Optional: method to update description if allowed by business rules
        internal void UpdateDescription(string newDescription)
        {
            if (string.IsNullOrWhiteSpace(newDescription))
                throw new AggregateException("Description cannot be empty.");

            Description = newDescription;
        }
    }
}
