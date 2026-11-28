using Marilog.Domain.Common;
using Marilog.Kernel.Enums;
using Marilog.Ship.Domain.Enums;

namespace Marilog.Ship.Domain.Entities
{
    /// <summary>
    /// Aggregate Root — one Master's General Account period. All totals are in the ship's base currency.
    ///
    /// Number 0 (IsOpening) holds the balances entered at the initial setup: the previous master
    /// account, each crew member's balance, the cash per currency and the cigarettes on board.
    /// Every later MGA takes its previous balance from the MGA before it.
    /// A normal MGA is saved only when it is closed: totals, lines and exchange rates are a snapshot.
    /// Reopening (unlock code) removes the last one, so its period is computed live again.
    /// </summary>
    public class Mga : Entity
    {
        public int Number { get; private set; }
        public string? Code { get; private set; }            // "10/2026", a second one in the same month "10/2026-B"; null for the opening
        public DateOnly FromDate { get; private set; }
        public DateOnly ToDate { get; private set; }
        public bool IsOpening { get; private set; }
        public MgaStatus Status { get; private set; } = MgaStatus.Draft;
        public string? EndReason { get; private set; }      // why the period is shorter than a month
        public string? MasterName { get; private set; }
        public string? Notes { get; private set; }          // reason for an opening / count difference
        // ── Summary of account (base currency) ───────────────────────────────────
        public decimal PreviousBalance { get; private set; }
        public decimal TotalWages { get; private set; }         // page 2 (net: wages + extras − deductions)
        public decimal TotalExpenses { get; private set; }      // page 3
        public decimal OfficeDeductions { get; private set; }   // page 5: cash/cigarettes drawn by office-paid crew
        public decimal TotalReceived { get; private set; }      // page 4
        public decimal ExchangeDifference { get; private set; } // foreign cash on board revalued at this MGA's rates
        public decimal CountDifference { get; private set; }    // expected − counted (+ shortage, − surplus)
        public decimal ClosingBalance { get; private set; }     // + master's favour, − owner's favour

        // ── Cigarettes on board (value only, base currency) ──────────────────────
        public decimal ExpectedCigarettes { get; private set; }
        public decimal? CountedCigarettes { get; private set; }

        public DateTime? ClosedAt { get; private set; }

        private readonly List<MgaCrewLine> _crewLines = new();
        private readonly List<MgaCashLine> _cashLines = new();
        public IReadOnlyCollection<MgaCrewLine> CrewLines => _crewLines.AsReadOnly();
        public IReadOnlyCollection<MgaCashLine> CashLines => _cashLines.AsReadOnly();

        private Mga() { }

        /// <summary>The one closing formula, used by the live draft and by the closed MGA.</summary>
        public static decimal ClosingOf(decimal previousBalance, decimal totalWages, decimal totalExpenses,
            decimal officeDeductions, decimal totalReceived, decimal exchangeDifference, decimal countDifference) =>
            previousBalance + totalWages + totalExpenses + officeDeductions - totalReceived
            + exchangeDifference + countDifference;

        // ── Normal MGA (saved when closed) ───────────────────────────────────────
        public static Mga CreateClosed(int number, string code, DateOnly from, DateOnly to,
            string? endReason, string? masterName, string? notes,
            decimal previousBalance, decimal totalWages, decimal totalExpenses, decimal officeDeductions,
            decimal totalReceived, decimal exchangeDifference, decimal countDifference,
            decimal expectedCigarettes, decimal countedCigarettes)
        {
            if (number <= 0) throw new ArgumentException("Invalid MGA number.");
            ArgumentException.ThrowIfNullOrWhiteSpace(code);
            if (to < from) throw new ArgumentException("The MGA cannot end before it starts.");
            if (countedCigarettes < 0) throw new ArgumentException("Cigarettes on board cannot be negative.");
            ShipGuard.Money(countedCigarettes, "Cigarettes on board");

            return new Mga
            {
                Number = number,
                Code = code.Trim(),
                FromDate = from,
                ToDate = to,
                EndReason = ShipGuard.Text(endReason),
                MasterName = ShipGuard.Text(masterName),
                Notes = ShipGuard.Text(notes),
                PreviousBalance = previousBalance,
                TotalWages = totalWages,
                TotalExpenses = totalExpenses,
                OfficeDeductions = officeDeductions,
                TotalReceived = totalReceived,
                ExchangeDifference = exchangeDifference,
                CountDifference = countDifference,
                ClosingBalance = ClosingOf(previousBalance, totalWages, totalExpenses, officeDeductions,
                    totalReceived, exchangeDifference, countDifference),
                ExpectedCigarettes = expectedCigarettes,
                CountedCigarettes = countedCigarettes,
                Status = MgaStatus.Closed,
                ClosedAt = DateTime.UtcNow
            };
        }

        /// <summary>One crew member's page-5 account. Only while the closed MGA is being created.</summary>
        public void AddClosingCrewLine(int crewContractId, WagesPaidBy wagesPaidBy, decimal monthlyWage, int days,
            decimal previousBalance, decimal wages, decimal extras, decimal cash, decimal cigarettes,
            decimal other, decimal paidAshore, decimal balance)
        {
            EnsureBeingCreated();
            ShipGuard.Id(crewContractId, nameof(MgaCrewLine.CrewContractId));
            if (_crewLines.Any(x => x.CrewContractId == crewContractId))
                throw new InvalidOperationException("A crew member appears more than once.");

            _crewLines.Add(MgaCrewLine.Create(Id, crewContractId, wagesPaidBy, monthlyWage, days,
                previousBalance, wages, extras, cash, cigarettes, other, paidAshore, balance));
        }

        /// <summary>Cash on board for one currency with the rate frozen for this MGA.</summary>
        public void AddClosingCashLine(int currencyId, decimal expected, decimal counted, decimal rateToBase)
        {
            EnsureBeingCreated();
            ShipGuard.Id(currencyId, nameof(MgaCashLine.CurrencyId));
            if (counted < 0) throw new ArgumentException("Cash on board cannot be negative.");
            ShipGuard.Money(counted, "Cash");
            if (rateToBase <= 0) throw new ArgumentException("Invalid exchange rate.");
            if (_cashLines.Any(x => x.CurrencyId == currencyId))
                throw new InvalidOperationException("A currency appears more than once.");

            _cashLines.Add(MgaCashLine.Create(Id, currencyId, expected, counted, rateToBase));
        }

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

        /// <summary>
        /// Locks the opening balances — called when the initial setup is completed.
        /// The rates of that day are kept, so the first MGA revalues the opening cash correctly.
        /// </summary>
        public void CloseOpening(string? notes, IReadOnlyDictionary<int, decimal> ratesToBase)
        {
            EnsureOpeningDraft();
            foreach (var line in _cashLines)
                if (ratesToBase.TryGetValue(line.CurrencyId, out var rate))
                    line.SetRate(rate);

            Notes = ShipGuard.Text(notes);
            Status = MgaStatus.Closed;
            ClosedAt = DateTime.UtcNow;
            Touch();
        }

        /// <summary>Opens the opening balances for one edit — only after a correct unlock code.</summary>
        public void ReopenOpening()
        {
            if (!IsOpening)
                throw new InvalidOperationException("Only the opening MGA can be reopened here.");
            if (Status != MgaStatus.Closed)
                throw new InvalidOperationException("The opening balances are not locked.");

            Status = MgaStatus.Draft;
            ClosedAt = null;
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

        private void EnsureBeingCreated()
        {
            if (IsOpening || Id != 0)
                throw new InvalidOperationException("Lines are added only while the closed MGA is created.");
        }
    }
}