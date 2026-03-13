using Marilog.Domain.Common;

namespace Marilog.Domain.Entities
{
    public class Port : Entity
    {
        public int PortID { get; private set; }
        public string PortCode { get; private set; } = null!;
        public string PortName { get; private set; } = null!;
        public int? CountryID { get; private set; }
        public Country? Country { get; private set; }


        public static Port Create(string portCode, string portName, int? countryId = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(portCode);
            ArgumentException.ThrowIfNullOrWhiteSpace(portName);

            return new Port
            {
                PortCode = portCode.ToUpperInvariant(),
                PortName = portName,
                CountryID = countryId
            };
        }

        public void Update(string portCode, string portName, int? countryId = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(portCode);
            ArgumentException.ThrowIfNullOrWhiteSpace(portName);

            PortCode = portCode.ToUpperInvariant();
            PortName = portName;
            CountryID = countryId;
            Touch();
        }
    }
}