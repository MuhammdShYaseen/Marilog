using Marilog.Domain.Common;
using Marilog.Domain.Entities.SystemEntities;
using Marilog.Ship.Domain.Enums;

namespace Marilog.Ship.Domain.Entities
{
    /// <summary>
    /// MGA page 4 — money the owner sent to the master or paid on his behalf.
    /// CashToMaster adds to the cash on board, Cigarettes to the cigarettes on board;
    /// CrewPaidAshore is also deducted from that crew member's account.
    /// </summary>
    public class OwnerRemittance : Entity
    {
        public DateOnly Date { get; private set; }
        public OwnerRemittanceType Type { get; private set; }
        public decimal Amount { get; private set; }
        public int CurrencyId { get; private set; }
        public Currency Currency { get; private set; } = null!;
        public int? CrewContractId { get; private set; }
        public CrewContract? CrewContract { get; private set; }
        public int? PortId { get; private set; }
        public Port? Port { get; private set; }
        public string? Reference { get; private set; }   // office document number
        public string? Notes { get; private set; }

        private OwnerRemittance() { }

        public static OwnerRemittance Create(DateOnly date, OwnerRemittanceType type, decimal amount,
            int currencyId, int? crewContractId = null, int? portId = null,
            string? reference = null, string? notes = null)
        {
            var remittance = new OwnerRemittance();
            remittance.Set(date, type, amount, currencyId, crewContractId, portId, reference, notes);
            return remittance;
        }

        public void Update(DateOnly date, OwnerRemittanceType type, decimal amount,
            int currencyId, int? crewContractId = null, int? portId = null,
            string? reference = null, string? notes = null)
        {
            Set(date, type, amount, currencyId, crewContractId, portId, reference, notes);
            Touch();
        }

        private void Set(DateOnly date, OwnerRemittanceType type, decimal amount,
            int currencyId, int? crewContractId, int? portId,
            string? reference, string? notes)
        {
            if (!Enum.IsDefined(type)) throw new ArgumentException("Invalid remittance type.");
            ShipGuard.PositiveMoney(amount, nameof(Amount));
            ShipGuard.Id(currencyId, nameof(CurrencyId));

            if (type == OwnerRemittanceType.CrewPaidAshore && crewContractId is null)
                throw new ArgumentException("Wages paid ashore need the crew member.");
            if (type != OwnerRemittanceType.CrewPaidAshore && crewContractId is not null)
                throw new ArgumentException("Only wages paid ashore are linked to a crew member.");

            Date = date;
            Type = type;
            Amount = amount;
            CurrencyId = currencyId;
            CrewContractId = crewContractId;
            PortId = portId;
            Reference = ShipGuard.Text(reference);
            Notes = ShipGuard.Text(notes);
        }
    }
}