

using Marilog.Contracts.DTOs.Responses;

namespace Marilog.Contracts.DTOs.EmailNotificationDTOs
{
    public class NotificationDataResponse
    {
        public IReadOnlyList<DocumentResponse> UnpaidDocuments { get; set; } = [];

        public IReadOnlyList<CertificateResponse> ExpiringVesselCertificates { get; set; } = [];

        public IReadOnlyList<CertificateResponse> ExpiredVesselCertificates { get; set; } = [];

        public IReadOnlyList<CertificateResponse> ExpiringPersonCertificates { get; set; } = [];

        public IReadOnlyList<CertificateResponse> ExpiredPersonCertificates { get; set; } = [];
    }
}
