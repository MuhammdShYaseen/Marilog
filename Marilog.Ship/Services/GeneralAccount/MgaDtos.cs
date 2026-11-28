using Marilog.Kernel.Enums;

namespace Marilog.Ship.Services.GeneralAccount
{
    public record MgaListItem(
        int Id,
        string Code,
        DateOnly FromDate,
        DateOnly ToDate,
        string? MasterName,
        decimal ClosingBalance,
        DateTime? ClosedAt,
        bool IsLast);   // only the last closed MGA can be reopened

    /// <summary>The period after the last closed MGA: it ends on any day from From to MaxTo (month end).</summary>
    public record MgaOpenPeriod(DateOnly From, DateOnly MaxTo, int BaseCurrencyId, string BaseCurrencyCode);

    /// <summary>
    /// The whole MGA. A draft is computed live with today's rates; a closed one uses its frozen
    /// rates and stored lines. All "Base" amounts are in the ship's base currency.
    /// </summary>
    public record MgaReport(
        int? MgaId,
        string Code,
        bool IsClosed,
        string VesselName,
        string? MasterName,
        string BaseCurrencyCode,
        DateOnly From,
        DateOnly To,
        string? EndReason,
        string? Notes,
        DateTime? ClosedAt,
        IReadOnlyList<MgaMovementLine> Movements,          // page 1
        IReadOnlyList<MgaWageLine> Wages,                  // page 2
        IReadOnlyList<MgaExpenseLine> Expenses,            // page 3
        IReadOnlyList<MgaExpenseLine> CigarettePurchases,  // cash → cigarettes, not an expense
        IReadOnlyList<MgaReceivedLine> Received,           // page 4
        IReadOnlyList<MgaCrewAccount> CrewAccounts,        // page 5
        IReadOnlyList<MgaCashItem> Cash,
        decimal PreviousCigarettes,
        decimal ExpectedCigarettes,
        decimal? CountedCigarettes,
        MgaSummary Summary);

    public record MgaMovementLine(
        string VoyageNumber,
        string PortName,
        DateTime? Arrival,
        DateTime? Departure,
        string? Purpose);

    /// <summary>Net = Wages + Extras − Deductions (same shape as Marilog payroll: Basic + Allowances − Deductions).</summary>
    public record MgaWageLine(
        int CrewContractId,
        string CrewName,
        string RankName,
        decimal MonthlyWage,
        int Days,
        decimal Wages,
        decimal Extras,
        decimal Deductions,
        decimal Net);

    public record MgaExpenseLine(
        DateOnly Date,
        string DocNumber,
        string DocTypeName,
        string? SupplierName,
        string? PortName,
        PaymentMethod Method,
        string? Description,
        string CurrencyCode,
        decimal Amount,
        decimal BaseAmount);

    public record MgaReceivedLine(
        DateOnly Date,
        string Description,
        string? PortName,
        string? Reference,
        string CurrencyCode,
        decimal Amount,
        decimal BaseAmount);

    /// <summary>One line of a crew member's account. BaseAmount: + owed to him, − taken / paid.</summary>
    public record MgaCrewEntry(
        DateTime Date,
        bool HasTime,
        string Kind,
        string? Description,
        string CurrencyCode,
        decimal Amount,
        decimal BaseAmount);

    public record MgaCrewAccount(
        int CrewContractId,
        string CrewName,
        string RankName,
        WagesPaidBy WagesPaidBy,
        decimal MonthlyWage,
        int Days,
        decimal PreviousBalance,
        decimal Wages,
        decimal Extras,
        decimal Cash,
        decimal Cigarettes,
        decimal Other,
        decimal PaidAshore,
        decimal Balance,
        IReadOnlyList<MgaCrewEntry> Entries);

    /// <summary>Cash for one currency (in that currency). RateToBase = base units per 1 unit.</summary>
    public record MgaCashItem(
        int CurrencyId,
        string CurrencyCode,
        decimal RateToBase,
        decimal Previous,
        decimal Expected,
        decimal? Counted);

    /// <summary>
    /// Closing = Previous + Wages + Expenses + Office deductions − Received + Exchange difference + Count difference.
    /// Final check: Closing − Crew balances + Cash on board + Cigarettes on board = Difference (should be 0).
    /// </summary>
    public record MgaSummary(
        decimal PreviousBalance,
        decimal TotalWages,
        decimal TotalExpenses,
        decimal OfficeDeductions,
        decimal TotalReceived,
        decimal ExchangeDifference,
        decimal CountDifference,
        decimal ClosingBalance,
        decimal CrewBalances,
        decimal CashOnBoard,
        decimal CigarettesOnBoard,
        decimal Difference);

    public record MgaCountItem(int CurrencyId, decimal Counted);

    /// <summary>
    /// EndReason when the period ends before the month end.
    /// Reason when the count differs from the records or the final check is not 0.
    /// </summary>
    public record CloseMgaRequest(
        DateOnly To,
        string? EndReason,
        IReadOnlyList<MgaCountItem> Cash,
        decimal CountedCigarettes,
        string? Reason);

    /// <summary>A master-paid crew member for the "Wages paid ashore" dialog.</summary>
    public record CrewPayAshoreItem(
        int CrewContractId,
        string CrewName,
        string RankName,
        decimal Balance,     // as at the date of the payment
        decimal MonthWage);  // wage of the chosen month (CrewWageRules)
}