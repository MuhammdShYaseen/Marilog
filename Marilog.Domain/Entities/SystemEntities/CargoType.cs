using Marilog.Domain.Common;
using Marilog.Kernel.Enums;

namespace Marilog.Domain.Entities.SystemEntities
{
    /// <summary>
    /// Lookup entity — user-defined cargo types (Wheat, Steel Coils, Sheep, Cars...).
    /// Category is fixed after creation: cargo extensions depend on it.
    /// </summary>
    public class CargoType : Entity
    {
        public string Code { get; private set; } = null!;          // e.g. "WHEAT"
        public string Name { get; private set; } = null!;          // e.g. "Wheat"
        public CargoCategory Category { get; private set; }
        public CargoUnit DefaultUnit { get; private set; }
        public int SortOrder { get; private set; }

        private CargoType() { }

        public static CargoType Create(string code, string name, CargoCategory category,
            CargoUnit defaultUnit, int sortOrder = 0)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(code);
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            if (!Enum.IsDefined(category))
                throw new ArgumentException("Invalid cargo category.");

            if (!Enum.IsDefined(defaultUnit))
                throw new ArgumentException("Invalid cargo unit.");

            return new CargoType
            {
                Code = code.Trim().ToUpperInvariant(),
                Name = name.Trim(),
                Category = category,
                DefaultUnit = defaultUnit,
                SortOrder = sortOrder
            };
        }

        public void Update(string name, CargoUnit defaultUnit, int sortOrder)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            if (!Enum.IsDefined(defaultUnit))
                throw new ArgumentException("Invalid cargo unit.");

            Name = name.Trim();
            DefaultUnit = defaultUnit;
            SortOrder = sortOrder;
            Touch();
        }
    }
}