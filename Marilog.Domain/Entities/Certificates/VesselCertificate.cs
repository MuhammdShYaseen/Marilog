using Marilog.Domain.Common;
using Marilog.Domain.ValueObjects.ReusableValueObjects;
using Marilog.Kernel.Enums;

namespace Marilog.Domain.Entities.Certificates
{
    public sealed class VesselCertificate : Entity
    {
        public VesselCertificateType Type { get; private set; }
        public Certificate Certificate { get; private set; } = null!;

        private VesselCertificate() { }

        public static VesselCertificate Create(VesselCertificateType type, Certificate certificate)
            => new()
            {
                Type = type,
                Certificate = certificate ?? throw new ArgumentNullException(nameof(certificate))
            };

        public void Update(VesselCertificateType type, Certificate certificate)
        {
            Type = type;
            Certificate = certificate ?? throw new ArgumentNullException(nameof(certificate));
        }
    }
}
