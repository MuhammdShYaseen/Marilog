namespace Marilog.Contracts.DTOs.Requests.Common
{
    public class UpsertCertificateRequest
    {
        public string CertificateName { get; set; } = null!;
        public DateOnly? IssueDate { get; set; }
        public DateOnly? ExpiryDate { get; set; }
        public string? Description { get; set; }
    }
}
