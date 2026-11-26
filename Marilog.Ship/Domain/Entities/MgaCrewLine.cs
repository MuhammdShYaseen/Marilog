using Marilog.Domain.Entities.SystemEntities;

namespace Marilog.Ship.Domain.Entities
{
    /// <summary>One crew member's account in an MGA (page 5), in the ship's base currency.</summary>
    public class MgaCrewLine
    {
        public int Id { get; private set; }
        public int MgaId { get; private set; }
        public int CrewContractId { get; private set; }
        public CrewContract CrewContract { get; private set; } = null!;

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
    }
}