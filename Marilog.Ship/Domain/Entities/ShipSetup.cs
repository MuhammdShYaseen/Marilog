using Marilog.Domain.Common;
using Marilog.Domain.Entities.SystemEntities;
using Marilog.Kernel.Enums;

namespace Marilog.Ship.Domain.Entities
{
    /// <summary>
    /// One row: the vessel this installation belongs to and its basic settings.
    /// Editable until the initial setup is completed.
    /// </summary>
    public class ShipSetup : Entity
    {
        public int VesselId { get; private set; }
        public Vessel Vessel { get; private set; } = null!;
        public int BaseCurrencyId { get; private set; }
        public Currency BaseCurrency { get; private set; } = null!;
        public WagesPaidBy DefaultWagesPaidBy { get; private set; }
        public bool IsCompleted { get; private set; }
        public DateTime? CompletedAt { get; private set; }

        private ShipSetup() { }

        public static ShipSetup Create(int vesselId, int baseCurrencyId, WagesPaidBy defaultWagesPaidBy)
        {
            ShipGuard.Id(vesselId, nameof(VesselId));
            ShipGuard.Id(baseCurrencyId, nameof(BaseCurrencyId));

            return new ShipSetup
            {
                VesselId = vesselId,
                BaseCurrencyId = baseCurrencyId,
                DefaultWagesPaidBy = defaultWagesPaidBy
            };
        }

        public void Update(int vesselId, int baseCurrencyId, WagesPaidBy defaultWagesPaidBy)
        {
            EnsureNotCompleted();
            ShipGuard.Id(vesselId, nameof(VesselId));
            ShipGuard.Id(baseCurrencyId, nameof(BaseCurrencyId));

            VesselId = vesselId;
            BaseCurrencyId = baseCurrencyId;
            DefaultWagesPaidBy = defaultWagesPaidBy;
            Touch();
        }

        public void Complete()
        {
            EnsureNotCompleted();
            IsCompleted = true;
            CompletedAt = DateTime.UtcNow;
            Touch();
        }

        private void EnsureNotCompleted()
        {
            if (IsCompleted)
                throw new InvalidOperationException("The initial setup is already completed and locked.");
        }
    }
}