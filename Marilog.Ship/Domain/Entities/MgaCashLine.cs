using Marilog.Domain.Entities.SystemEntities;

namespace Marilog.Ship.Domain.Entities
{
    /// <summary>Cash on board for one currency in an MGA: expected from the records vs the physical count.</summary>
    public class MgaCashLine
    {
        public int Id { get; private set; }
        public int MgaId { get; private set; }
        public int CurrencyId { get; private set; }
        public Currency Currency { get; private set; } = null!;
        public decimal Expected { get; private set; }
        public decimal? Counted { get; private set; }

        private MgaCashLine() { }

        internal static MgaCashLine CreateOpening(int mgaId, int currencyId, decimal amount)
            => new()
            {
                MgaId = mgaId,
                CurrencyId = currencyId,
                Expected = amount,
                Counted = amount
            };
    }
}