using Marilog.Kernel.Enums;

namespace Marilog.Contracts.DTOs.Requests.PersonDTOs
{
    public sealed class PersonCertificateFilter
    {
        public int? PersonId { get; set; }
        public CertificateExpiryStatus? Status { get; set; }
        public PersonCertificateType? Type { get; set; }
        public bool IncludeInactivePersons { get; set; }
    }
}