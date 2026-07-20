using Marilog.Domain.Common;
using Marilog.Domain.ValueObjects.ReusableValueObjects;
using Marilog.Kernel.Enums;

namespace Marilog.Domain.Entities.Certificates
{
    public sealed class PersonCertificate : Entity // أو ValueObject لو مفيش هوية مستقلة مطلوبة
    {
        public PersonCertificateType Type { get; private set; }
        public Certificate Certificate { get; private set; } = null!;

        private PersonCertificate() { }

        public static PersonCertificate Create(PersonCertificateType type, Certificate certificate) => new()
        {
             Type = type,
             Certificate = certificate ?? throw new ArgumentNullException(nameof(certificate))
        };

        public void Update(PersonCertificateType type, Certificate certificate)
        {
            Type = type;
            Certificate = certificate ?? throw new ArgumentNullException(nameof(certificate));
        }
    }
}
