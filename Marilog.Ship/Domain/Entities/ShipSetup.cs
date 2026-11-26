using Marilog.Domain.Common;
using Marilog.Domain.Entities.SystemEntities;
using Marilog.Kernel.Enums;

namespace Marilog.Ship.Domain.Entities
{
    /// <summary>
    /// One row: the vessel this installation belongs to and its basic settings.
    /// Editable until the initial setup is completed.
    /// The ship's base currency is the Currency marked IsBaseCurrency (same as Marilog).
    /// </summary>
    public class ShipSetup : Entity
    {
        public int VesselId { get; private set; }
        public Vessel Vessel { get; private set; } = null!;
        public WagesPaidBy DefaultWagesPaidBy { get; private set; }
        public bool IsCompleted { get; private set; }
        public DateTime? CompletedAt { get; private set; }
        public string? ShipKeyEncrypted { get; private set; }   // key from the office, encrypted (ShipKeyProtector)

        private ShipSetup() { }

        public static ShipSetup Create(int vesselId, WagesPaidBy defaultWagesPaidBy)
        {
            ShipGuard.Id(vesselId, nameof(VesselId));
            if (!Enum.IsDefined(defaultWagesPaidBy)) throw new ArgumentException("Invalid WagesPaidBy.");

            return new ShipSetup
            {
                VesselId = vesselId,
                DefaultWagesPaidBy = defaultWagesPaidBy
            };
        }

        public void Update(WagesPaidBy defaultWagesPaidBy)
        {
            EnsureNotCompleted();
            if (!Enum.IsDefined(defaultWagesPaidBy)) throw new ArgumentException("Invalid WagesPaidBy.");

            DefaultWagesPaidBy = defaultWagesPaidBy;
            Touch();
        }

        public void SetShipKey(string shipKeyEncrypted)
        {
            EnsureNotCompleted();
            ArgumentException.ThrowIfNullOrWhiteSpace(shipKeyEncrypted);
            ShipKeyEncrypted = shipKeyEncrypted;
            Touch();
        }

        public void Complete()
        {
            EnsureNotCompleted();
            if (ShipKeyEncrypted is null)
                throw new InvalidOperationException("Enter the ship key from the office before completing the setup.");

            IsCompleted = true;
            CompletedAt = DateTime.UtcNow;
            Touch();
        }

        /// <summary>Opens the setup for one edit — only after a correct unlock code.</summary>
        public void Reopen()
        {
            if (!IsCompleted)
                throw new InvalidOperationException("The setup is not locked.");

            IsCompleted = false;
            CompletedAt = null;
            Touch();
        }

        private void EnsureNotCompleted()
        {
            if (IsCompleted)
                throw new InvalidOperationException("The initial setup is already completed and locked.");
        }
    }
}