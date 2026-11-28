using Marilog.Domain.Common;
using Marilog.Domain.Entities.SystemEntities;
using Marilog.Ship.Domain.Enums;

namespace Marilog.Ship.Domain.Entities
{
    /// <summary>MGA page 2 — extra payable on top of wages (bonus, travel ticket, ...). Reason is mandatory.</summary>
    public class CrewExtra : Entity
    {
        public DateOnly Date { get; private set; }
        public int CrewContractId { get; private set; }
        public CrewContract CrewContract { get; private set; } = null!;
        public CrewExtraType Type { get; private set; }
        public decimal Amount { get; private set; }
        public int CurrencyId { get; private set; }
        public Currency Currency { get; private set; } = null!;
        public string Reason { get; private set; } = null!;

        private CrewExtra() { }

        public static CrewExtra Create(DateOnly date, int crewContractId, CrewExtraType type,
            decimal amount, int currencyId, string reason)
        {
            var extra = new CrewExtra();
            extra.Set(date, crewContractId, type, amount, currencyId, reason);
            return extra;
        }

        public void Update(DateOnly date, int crewContractId, CrewExtraType type,
            decimal amount, int currencyId, string reason)
        {
            Set(date, crewContractId, type, amount, currencyId, reason);
            Touch();
        }

        private void Set(DateOnly date, int crewContractId, CrewExtraType type,
            decimal amount, int currencyId, string reason)
        {
            if (!Enum.IsDefined(type)) throw new ArgumentException("Invalid extra type.");
            ShipGuard.Id(crewContractId, nameof(CrewContractId));
            ShipGuard.PositiveMoney(amount, nameof(Amount));
            ShipGuard.Id(currencyId, nameof(CurrencyId));
            ArgumentException.ThrowIfNullOrWhiteSpace(reason);

            Date = date;
            CrewContractId = crewContractId;
            Type = type;
            Amount = amount;
            CurrencyId = currencyId;
            Reason = reason.Trim();
        }
    }
}