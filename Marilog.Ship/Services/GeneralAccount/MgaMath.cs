using MgaEntity = Marilog.Ship.Domain.Entities.Mga;

namespace Marilog.Ship.Services.GeneralAccount
{
    /// <summary>The MGA arithmetic, in one place: used by the service and by the page while counting.</summary>
    public static class MgaMath
    {
        /// <summary>Amount in base currency, rounded once per amount (2 decimals, away from zero).</summary>
        public static decimal ToBase(decimal amount, decimal rateToBase) =>
            Math.Round(amount * rateToBase, 2, MidpointRounding.AwayFromZero);

        public static MgaSummary Summarize(decimal previousBalance, decimal totalWages, decimal totalExpenses,
            decimal officeDeductions, decimal totalReceived, decimal exchangeDifference, decimal crewBalances,
            decimal expectedCash, decimal expectedCigarettes, decimal countedCash, decimal countedCigarettes)
        {
            // A shortage is cash the owner no longer has on board: + to the master's account.
            var countDifference = (expectedCash - countedCash) + (expectedCigarettes - countedCigarettes);
            var closing = MgaEntity.ClosingOf(previousBalance, totalWages, totalExpenses, officeDeductions,
                totalReceived, exchangeDifference, countDifference);

            return new MgaSummary(previousBalance, totalWages, totalExpenses, officeDeductions, totalReceived,
                exchangeDifference, countDifference, closing, crewBalances, countedCash, countedCigarettes,
                closing - crewBalances + countedCash + countedCigarettes);
        }

        /// <summary>The summary of a draft with the physical count; a currency not counted yet counts as expected.</summary>
        public static MgaSummary Recount(MgaReport draft, IReadOnlyDictionary<int, decimal> countedCash,
            decimal countedCigarettes)
        {
            var s = draft.Summary;
            var expected = draft.Cash.Sum(c => ToBase(c.Expected, c.RateToBase));
            var counted = draft.Cash.Sum(c =>
                ToBase(countedCash.TryGetValue(c.CurrencyId, out var n) ? n : c.Expected, c.RateToBase));

            return Summarize(s.PreviousBalance, s.TotalWages, s.TotalExpenses, s.OfficeDeductions,
                s.TotalReceived, s.ExchangeDifference, s.CrewBalances,
                expected, draft.ExpectedCigarettes, counted, countedCigarettes);
        }
    }
}