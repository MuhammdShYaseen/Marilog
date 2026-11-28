using Marilog.Domain.Entities.SystemEntities;

namespace Marilog.Ship.Domain.Entities
{
    /// <summary>
    /// Cash on board for one currency in an MGA: expected from the records vs the physical count.
    /// RateToBase (base units per 1 unit) is frozen when the MGA is closed.
    /// </summary>
    public class MgaCashLine
    {
        public int Id { get; private set; }
        public int MgaId { get; private set; }
        public int CurrencyId { get; private set; }
        public Currency Currency { get; private set; } = null!;
        public decimal Expected { get; private set; }
        public decimal? Counted { get; private set; }
        public decimal? RateToBase { get; private set; }   // null only on an opening closed before rates were kept

        private MgaCashLine() { }

        internal static MgaCashLine CreateOpening(int mgaId, int currencyId, decimal amount)
            => new()
            {
                MgaId = mgaId,
                CurrencyId = currencyId,
                Expected = amount,
                Counted = amount
            };

        internal static MgaCashLine Create(int mgaId, int currencyId, decimal expected, decimal counted, decimal rateToBase)
            => new()
            {
                MgaId = mgaId,
                CurrencyId = currencyId,
                Expected = expected,
                Counted = counted,
                RateToBase = rateToBase
            };

        internal void SetRate(decimal rateToBase)
        {
            if (rateToBase <= 0) throw new ArgumentException("Invalid exchange rate.");
            RateToBase = rateToBase;
        }
    }
}