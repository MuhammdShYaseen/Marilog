

using Marilog.Contracts.DTOs.Responses;

namespace Marilog.Contracts.DTOs.EmailNotificationDTOs
{
    public class NotificationDataResponse
    {
        public IReadOnlyList<DocumentResponse> UnpaidDocuments { get; init; } = [];

        public IReadOnlyList<CertificateResponse> ExpiringVesselCertificates { get; init; } = [];

        public IReadOnlyList<CertificateResponse> ExpiredVesselCertificates { get; init; } = [];

        public IReadOnlyList<CertificateResponse> ExpiringPersonCertificates { get; init; } = [];

        public IReadOnlyList<CertificateResponse> ExpiredPersonCertificates { get; init; } = [];
    }
}
