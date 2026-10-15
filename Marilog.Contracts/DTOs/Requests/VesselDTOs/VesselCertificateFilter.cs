using Marilog.Kernel.Enums;

namespace Marilog.Contracts.DTOs.Requests.VesselDTOs
{
    public sealed class VesselCertificateFilter
    {
        public int? VesselId { get; set; }
        public CertificateExpiryStatus? Status { get; set; }
        public VesselCertificateType? Type { get; set; }
        public bool IncludeInactiveVessels { get; set; }
    }
}