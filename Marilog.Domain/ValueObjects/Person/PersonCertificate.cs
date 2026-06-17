
namespace Marilog.Domain.ValueObjects.Person
{
    public sealed class PersonCertificate : ValueObject
    {
        public string CertificateName { get; private set; } = null!;
        public DateOnly? IssueDate { get; private set; }
        public DateOnly? ExpiryDate { get; private set; }
        public string? Description { get; private set; }

        private PersonCertificate() { }

        public static PersonCertificate Create(string name,
            DateOnly? issueDate = null,
            DateOnly? expiryDate = null,
            string? description = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            return new PersonCertificate
            {
                CertificateName = name.Trim(),
                IssueDate = issueDate,
                ExpiryDate = expiryDate,
                Description = description?.Trim()
            };
        }

        // ValueObjects immutable — بدل Update نرجع instance جديد
        public PersonCertificate WithUpdates(string name,
            DateOnly? issueDate,
            DateOnly? expiryDate,
            string? description)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            return new PersonCertificate
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
