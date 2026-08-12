namespace Marilog.Contracts.DTOs.EmailNotificationDTOs
{
    public sealed class NotificationSettingsOptions
    {
        public bool UnpaidDocuments { get; set; }

        public bool ExpiringVesselCertificates { get; set; }

        public bool ExpiredVesselCertificates { get; set; }

        public bool ExpiringPersonCertificates { get; set; }

        public bool ExpiredPersonCertificates { get; set; }
    }
}
