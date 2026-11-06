using Marilog.Domain.Common;

namespace Marilog.Domain.Entities.SystemEntities
{
    /// <summary>
    /// Lookup entity — user-defined vessel types (Bulk Carrier, Livestock Carrier, RoRo...).
    /// Informational only: cargo behaviour is driven by CargoType.Category, not by the vessel type.
    /// </summary>
    public class VesselType : Entity
    {
        public string Code { get; private set; } = null!;   // e.g. "BULK"
        public string Name { get; private set; } = null!;   // e.g. "Bulk Carrier"
        public int SortOrder { get; private set; }

        private VesselType() { }

        public static VesselType Create(string code, string name, int sortOrder = 0)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(code);
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            return new VesselType
            {
                Code = code.Trim().ToUpperInvariant(),
                Name = name.Trim(),
                SortOrder = sortOrder
            };
        }

        public void Update(string name, int sortOrder)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            Name = name.Trim();
            SortOrder = sortOrder;
            Touch();
        }
    }
}