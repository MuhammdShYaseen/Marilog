namespace Marilog.Domain.ValueObjects.ReusableValueObjects
{
    public sealed class Certificate : ValueObject
    {
        public string CertificateName { get; private set; } = null!;
        public string? CertificateNumber { get; private set; }
        public string? IssuingAuthority { get; private set; }
        public DateOnly? IssueDate { get; private set; }
        public DateOnly? ExpiryDate { get; private set; }
        public string? Description { get; private set; }

        private Certificate() { }

        public static Certificate Create(
            string name,
            string? certificateNumber = null,
            string? issuingAuthority = null,
            DateOnly? issueDate = null,
            DateOnly? expiryDate = null,
            string? description = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            if (issueDate.HasValue && expiryDate.HasValue && expiryDate < issueDate)
                throw new ArgumentException("Expiry date cannot precede issue date.");

            return new Certificate
            {
                CertificateName = name.Trim(),
                CertificateNumber = certificateNumber?.Trim(),
                IssuingAuthority = issuingAuthority?.Trim(),
                IssueDate = issueDate,
                ExpiryDate = expiryDate,
                Description = description?.Trim()
            };
        }

        // ValueObjects immutable — بدل Update نرجع instance جديد
        public Certificate WithUpdates(
            string name,
            string? certificateNumber,
            string? issuingAuthority,
            DateOnly? issueDate,
            DateOnly? expiryDate,
            string? description)
            => Create(name, certificateNumber, issuingAuthority, issueDate, expiryDate, description);

        public bool IsExpired(DateOnly asOf)
            => ExpiryDate.HasValue && ExpiryDate.Value < asOf;

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return CertificateName;
            yield return CertificateNumber;
            yield return IssuingAuthority;
            yield return IssueDate;
            yield return ExpiryDate;
        }
    }
}