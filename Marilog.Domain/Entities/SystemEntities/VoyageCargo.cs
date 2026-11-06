using Marilog.Domain.ValueObjects.Cargo;
using Marilog.Kernel.Enums;

namespace Marilog.Domain.Entities.SystemEntities
{
    /// <summary>
    /// Core cargo line — shared by every trade (bulk, general, livestock, RoRo, container, tanker).
    /// Trade-specific data lives in optional extensions (Bulk, General).
    /// Future extensions (container tracking, vehicle VINs...) reference VoyageCargoID
    /// without changing this class.
    /// </summary>
    public class VoyageCargo
    {
        public int VoyageCargoID { get; private set; }
        public int StopID { get; private set; }
        public int CargoTypeID { get; private set; }
        public CargoType CargoType { get; private set; } = null!;
        public CargoOperation Operation { get; private set; }
        public decimal Quantity { get; private set; }
        public CargoUnit Unit { get; private set; }
        public decimal? WeightMt { get; private set; }
        public int? ContractID { get; private set; }
        public string? Notes { get; private set; }

        // ── Extensions (optional, one per category) ──
        public BulkCargoDetail? Bulk { get; private set; }
        public GeneralCargoDetail? General { get; private set; }

        private VoyageCargo() { }   // EF Core

        internal static VoyageCargo Create(int cargoTypeId, CargoCategory category,
            CargoOperation operation, decimal quantity, CargoUnit unit,
            decimal? weightMt, int? contractId, string? notes,
            BulkCargoDetail? bulk, GeneralCargoDetail? general)
        {
            var line = new VoyageCargo();
            line.Set(cargoTypeId, category, operation, quantity, unit, weightMt, contractId, notes, bulk, general);
            return line;
        }

        internal void Update(int cargoTypeId, CargoCategory category,
            CargoOperation operation, decimal quantity, CargoUnit unit,
            decimal? weightMt, int? contractId, string? notes,
            BulkCargoDetail? bulk, GeneralCargoDetail? general)
            => Set(cargoTypeId, category, operation, quantity, unit, weightMt, contractId, notes, bulk, general);

        private void Set(int cargoTypeId, CargoCategory category,
            CargoOperation operation, decimal quantity, CargoUnit unit,
            decimal? weightMt, int? contractId, string? notes,
            BulkCargoDetail? bulk, GeneralCargoDetail? general)
        {
            if (cargoTypeId <= 0)
                throw new ArgumentException("Cargo type is required.");

            if (!Enum.IsDefined(operation))
                throw new ArgumentException("Invalid cargo operation.");

            if (!Enum.IsDefined(unit))
                throw new ArgumentException("Invalid cargo unit.");

            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero.");

            if (weightMt < 0)
                throw new ArgumentException("Weight cannot be negative.");

            if (bulk is not null && category != CargoCategory.DryBulk)
                throw new InvalidOperationException("Bulk details apply to dry bulk cargo only.");

            if (general is not null && category != CargoCategory.GeneralCargo)
                throw new InvalidOperationException("General cargo details apply to general cargo only.");

            CargoTypeID = cargoTypeId;
            Operation = operation;
            Quantity = quantity;
            Unit = unit;
            WeightMt = unit == CargoUnit.MT ? quantity : weightMt;   // MT lines: weight = quantity
            ContractID = contractId;
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
            Bulk = bulk;
            General = general;
        }
    }
}