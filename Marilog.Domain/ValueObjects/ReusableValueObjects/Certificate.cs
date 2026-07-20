namespace Marilog.Domain.ValueObjects.ReusableValueObjects
{
    public sealed class Certificate : ValueObject
    {
        public string CertificateName { get; private set; } = null!;
        public DateOnly? IssueDate { get; private set; }
        public DateOnly? ExpiryDate { get; private set; }
        public string? Description { get; private set; }

        private Certificate() { }

        public static Certificate Create(string name,
            DateOnly? issueDate = null,
            DateOnly? expiryDate = null,
            string? description = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            return new Certificate
            {
                CertificateName = name.Trim(),
                IssueDate = issueDate,
                ExpiryDate = expiryDate,
                Description = description?.Trim()
            };
        }

        // ValueObjects immutable — بدل Update نرجع instance جديد
        public Certificate WithUpdates(string name,
            DateOnly? issueDate,
            DateOnly? expiryDate,
            string? description)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            return new Certificate
            {
                CertificateName = name.Trim(),
                IssueDate = issueDate,
                ExpiryDate = expiryDate,
                Description = description?.Trim()
            };
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return CertificateName;
            yield return IssueDate;
            yield return ExpiryDate;
        }
    }
}
