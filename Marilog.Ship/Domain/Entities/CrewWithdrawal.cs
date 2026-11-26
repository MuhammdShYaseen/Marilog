using Marilog.Domain.Common;
using Marilog.Domain.Entities.SystemEntities;
using Marilog.Ship.Domain.Enums;

namespace Marilog.Ship.Domain.Entities
{
    /// <summary>
    /// MGA page 5 — what a crew member took on board.
    /// Cash leaves the ship's cash, Cigarettes leave the cigarettes on board;
    /// Other is a deduction without cash and needs a type and a reason.
    /// </summary>
    public class CrewWithdrawal : Entity
    {
        public DateOnly Date { get; private set; }
        public int CrewContractId { get; private set; }
        public CrewContract CrewContract { get; private set; } = null!;
        public CrewWithdrawalType Type { get; private set; }
        public int? DeductionTypeId { get; private set; }
        public DeductionType? DeductionType { get; private set; }
        public decimal Amount { get; private set; }
        public int CurrencyId { get; private set; }
        public Currency Currency { get; private set; } = null!;
        public decimal ExchangeRate { get; private set; }
        public string? Reason { get; private set; }

        private CrewWithdrawal() { }

        public static CrewWithdrawal Create(DateOnly date, int crewContractId, CrewWithdrawalType type,
            decimal amount, int currencyId, decimal exchangeRate,
            int? deductionTypeId = null, string? reason = null)
        {
            var withdrawal = new CrewWithdrawal();
            withdrawal.Set(date, crewContractId, type, amount, currencyId, exchangeRate, deductionTypeId, reason);
            return withdrawal;
        }

        public void Update(DateOnly date, int crewContractId, CrewWithdrawalType type,
            decimal amount, int currencyId, decimal exchangeRate,
            int? deductionTypeId = null, string? reason = null)
        {
            Set(date, crewContractId, type, amount, currencyId, exchangeRate, deductionTypeId, reason);
            Touch();
        }

        private void Set(DateOnly date, int crewContractId, CrewWithdrawalType type,
            decimal amount, int currencyId, decimal exchangeRate,
            int? deductionTypeId, string? reason)
        {
            if (!Enum.IsDefined(type)) throw new ArgumentException("Invalid withdrawal type.");
            ShipGuard.Id(crewContractId, nameof(CrewContractId));
            ShipGuard.PositiveMoney(amount, nameof(Amount));
            ShipGuard.Id(currencyId, nameof(CurrencyId));
            ShipGuard.ExchangeRate(exchangeRate);

            if (type == CrewWithdrawalType.Other)
            {
                if (deductionTypeId is null)
                    throw new ArgumentException("An 'Other' deduction needs a deduction type.");
                if (string.IsNullOrWhiteSpace(reason))
                    throw new ArgumentException("An 'Other' deduction needs a reason.");
            }
            else if (deductionTypeId is not null)
            {
                throw new ArgumentException("Deduction type is only used for 'Other' deductions.");
            }

            Date = date;
            CrewContractId = crewContractId;
            Type = type;
            DeductionTypeId = deductionTypeId;
            Amount = amount;
            CurrencyId = currencyId;
            ExchangeRate = exchangeRate;
            Reason = ShipGuard.Text(reason);
        }
    }
}