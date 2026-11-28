using Marilog.Domain.Entities.SystemEntities;
using Marilog.Kernel.Enums;

namespace Marilog.Ship.Domain.Entities
{
    /// <summary>
    /// One crew member's account in an MGA (page 5), in the ship's base currency.
    /// Master-paid: Balance = Previous + Wages + Extras − Cash − Cigarettes − Other − PaidAshore.
    /// Office-paid: the draws are for office deduction, the balance does not move.
    /// </summary>
    public class MgaCrewLine
    {
        public int Id { get; private set; }
        public int MgaId { get; private set; }
        public int CrewContractId { get; private set; }
        public CrewContract CrewContract { get; private set; } = null!;

        public WagesPaidBy? WagesPaidBy { get; private set; }   // null on the opening lines
        public decimal MonthlyWage { get; private set; }        // as it was when the MGA was closed
        public int Days { get; private set; }
        public decimal PreviousBalance { get; private set; }
        public decimal Wages { get; private set; }
        public decimal Extras { get; private set; }
        public decimal Cash { get; private set; }
        public decimal Cigarettes { get; private set; }
        public decimal Other { get; private set; }
        public decimal PaidAshore { get; private set; }
        public decimal Balance { get; private set; }   // + owed to him, − advance

        private MgaCrewLine() { }

        internal static MgaCrewLine CreateOpening(int mgaId, int crewContractId, decimal balance)
            => new()
            {
                MgaId = mgaId,
                CrewContractId = crewContractId,
                Balance = balance
            };

        internal static MgaCrewLine Create(int mgaId, int crewContractId, WagesPaidBy wagesPaidBy,
            decimal monthlyWage, int days, decimal previousBalance, decimal wages, decimal extras,
            decimal cash, decimal cigarettes, decimal other, decimal paidAshore, decimal balance)
            => new()
            {
                MgaId = mgaId,
                CrewContractId = crewContractId,
                WagesPaidBy = wagesPaidBy,
                MonthlyWage = monthlyWage,
                Days = days,
                PreviousBalance = previousBalance,
                Wages = wages,
                Extras = extras,
                Cash = cash,
                Cigarettes = cigarettes,
                Other = other,
                PaidAshore = paidAshore,
                Balance = balance
            };
    }
}