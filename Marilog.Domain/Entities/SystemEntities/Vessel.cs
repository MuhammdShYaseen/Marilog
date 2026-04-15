using Marilog.Domain.Common;

namespace Marilog.Domain.Entities.SystemEntities
{
    public class Vessel : Entity
    {
        public int CompanyID { get; private set; }
        public Company Company { get; private set; } = null!;
        public string VesselName { get; private set; } = null!;
        public string? IMONumber { get; private set; }
        public decimal? GrossTonnage { get; private set; }
        public int? FlagCountryID { get; private set; }
        public Country? FlagCountry { get; private set; }
        public string? Notes { get; private set; }

        private readonly List<CrewContract> _crewContracts = new();
        public IReadOnlyCollection<CrewContract> CrewContracts => _crewContracts.AsReadOnly();

        private Vessel() { }
        public static Vessel Create(int companyId, string vesselName,
            string? imoNumber = null, decimal? grossTonnage = null,
            int? flagCountryId = null, string? notes = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(vesselName);
            if (companyId <= 0) throw new ArgumentException("Invalid CompanyID.");

            return new Vessel
            {
                CompanyID = companyId,
                VesselName = vesselName,
                IMONumber = imoNumber,
                GrossTonnage = grossTonnage,
                FlagCountryID = flagCountryId,
                Notes = notes
            };
        }

        public void Update(string vesselName, string? imoNumber = null,
            decimal? grossTonnage = null, int? flagCountryId = null, string? notes = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(vesselName);

            VesselName = vesselName;
            IMONumber = imoNumber;
            GrossTonnage = grossTonnage;
            FlagCountryID = flagCountryId;
            Notes = notes;
            Touch();
        }
    }
}