

using Marilog.Kernel.Enums;

namespace Marilog.Contracts.DTOs.Responses
{
    public class CertificateResponse
    {
        public int Index { get; set; }
        public string CertificateName { get; set; } = string.Empty;
        public string? CertificateNumber { get; set; }
        public string? IssuingAuthority { get; set; }
        public DateOnly? IssueDate { get; set; }
        public DateOnly? ExpiryDate { get; set; }
        public string? Description { get; set; }
        public PersonCertificateType? PType { get; set; }
        public VesselCertificateType? VType { get; set; }
    }
}
