using Marilog.Domain.Common;
using Marilog.Ship.Domain.Enums;

namespace Marilog.Ship.Domain.Entities
{
    /// <summary>
    /// Aggregate Root — one Master's General Account period. All totals are in the ship's base currency.
    ///
    /// Number 0 (IsOpening) holds the balances entered at the initial setup: the previous master
    /// account, each crew member's balance, the cash per currency and the cigarettes on board.
    /// Every later MGA takes its previous balance from the MGA before it.
    /// Totals and lines are a snapshot taken when the MGA is closed.
    /// </summary>
    public class Mga : Entity
    {
        public int Number { get; private set; }
        public DateOnly FromDate { get; private set; }
        public DateOnly ToDate { get; private set; }
        public bool IsOpening { get; private set; }
        public MgaStatus Status { get; private set; } = MgaStatus.Draft;
        public string? EndReason { get; private set; }      // why the period is shorter than a month
        public string? MasterName { get; private set; }

        // ── Summary of account (base currency) ───────────────────────────────────
        public decimal PreviousBalance { get; private set; }
        public decimal TotalWages { get; private set; }      // page 2
        public decimal TotalExpenses { get; private set; }   // page 3
        public decimal TotalReceived { get; private set; }   // page 4
        public decimal ClosingBalance { get; private set; }  // + master's favour, − owner's favour

        // ── Cigarettes on board (value only, base currency) ──────────────────────
        public decimal ExpectedCigarettes { get; private set; }
        public decimal? CountedCigarettes { get; private set; }

        public DateTime? ClosedAt { get; private set; }

        private readonly List<MgaCrewLine> _crewLines = new();
        private readonly List<MgaCashLine> _cashLines = new();
        public IReadOnlyCollection<MgaCrewLine> CrewLines => _crewLines.AsReadOnly();
        public IReadOnlyCollection<MgaCashLine> CashLines => _cashLines.AsReadOnly();

        private Mga() { }

        // ── Opening balances (initial setup) ─────────────────────────────────────
        public static Mga CreateOpening(DateOnly asOf, string? masterName,
            decimal masterAccountBalance, decimal cigarettesOnBoard)
        {
            var mga = new Mga { Number = 0, IsOpening = true };
            mga.SetOpening(asOf, masterName, masterAccountBalance, cigarettesOnBoard);
            return mga;
        }

        public void UpdateOpening(DateOnly asOf, string? masterName,
            decimal masterAccountBalance, decimal cigarettesOnBoard)
        {
            EnsureOpeningDraft();
            SetOpening(asOf, masterName, masterAccountBalance, cigarettesOnBoard);
            Touch();
        }

        /// <summary>Adds or replaces a crew member's opening balance (+ owed to him, − advance).</summary>
        public void SetOpeningCrewBalance(int crewContractId, decimal balance)
        {
            EnsureOpeningDraft();
            ShipGuard.Id(crewContractId, nameof(MgaCrewLine.CrewContractId));
            ShipGuard.Money(balance, "Balance");

            _crewLines.RemoveAll(x => x.CrewContractId == crewContractId);
            _crewLines.Add(MgaCrewLine.CreateOpening(Id, crewContractId, balance));
            Touch();
        }

        public void RemoveOpeningCrewBalance(int crewContractId)
        {
            EnsureOpeningDraft();
            _crewLines.RemoveAll(x => x.CrewContractId == crewContractId);
            Touch();
        }

        /// <summary>Adds or replaces the opening cash for one currency.</summary>
        public void SetOpeningCash(int currencyId, decimal amount)
        {
            EnsureOpeningDraft();
            ShipGuard.Id(currencyId, nameof(MgaCashLine.CurrencyId));
            if (amount < 0) throw new ArgumentException("Cash on board cannot be negative.");
            ShipGuard.Money(amount, "Cash");

            _cashLines.RemoveAll(x => x.CurrencyId == currencyId);
            _cashLines.Add(MgaCashLine.CreateOpening(Id, currencyId, amount));
            Touch();
        }

        public void RemoveOpeningCash(int currencyId)
        {
            EnsureOpeningDraft();
            _cashLines.RemoveAll(x => x.CurrencyId == currencyId);
            Touch();
        }

        /// <summary>Locks the opening balances — called when the initial setup is completed.</summary>
        public void CloseOpening()
        {
            EnsureOpeningDraft();
            Status = MgaStatus.Closed;
            ClosedAt = DateTime.UtcNow;
            Touch();
        }

        // ── Private ──────────────────────────────────────────────────────────────
        private void SetOpening(DateOnly asOf, string? masterName,
            decimal masterAccountBalance, decimal cigarettesOnBoard)
        {
            ShipGuard.Money(masterAccountBalance, "Master account balance");
            if (cigarettesOnBoard < 0) throw new ArgumentException("Cigarettes on board cannot be negative.");
            ShipGuard.Money(cigarettesOnBoard, "Cigarettes on board");

            FromDate = asOf;
            ToDate = asOf;
            MasterName = ShipGuard.Text(masterName);
            ClosingBalance = masterAccountBalance;
            ExpectedCigarettes = cigarettesOnBoard;
            CountedCigarettes = cigarettesOnBoard;
        }

        private void EnsureOpeningDraft()
        {
            if (!IsOpening)
                throw new InvalidOperationException("Only the opening MGA holds opening balances.");
            if (Status != MgaStatus.Draft)
                throw new InvalidOperationException("The opening balances are locked.");
        }
    }
}