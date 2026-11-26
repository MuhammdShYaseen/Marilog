using Marilog.Kernel.Enums;

namespace Marilog.Ship.Services.Setup
{
    public record ShipSetupStatusResponse(
        bool IsCompleted,
        int? VesselId,
        string? VesselName,
        string? OwnerCompanyName,
        string? ImoNumber,
        decimal? GrossTonnage,
        int? FlagCountryId,
        int? VesselTypeId,
        WagesPaidBy DefaultWagesPaidBy,
        int? BaseCurrencyId,
        string? BaseCurrencyCode,
        bool HasOpening);

    public record SaveShipVesselRequest(
        string OwnerCompanyName,
        string VesselName,
        string? ImoNumber,
        decimal? GrossTonnage,
        int? FlagCountryId,
        int? VesselTypeId,
        int BaseCurrencyId,
        WagesPaidBy DefaultWagesPaidBy);

    // All balances in the ship's base currency; cash in its own currency.
    public record OpeningCrewBalanceItem(int CrewContractId, decimal Balance);
    public record OpeningCashItem(int CurrencyId, decimal Amount);

    public record SaveOpeningBalancesRequest(
        DateOnly AsOf,
        decimal MasterAccountBalance,
        decimal CigarettesOnBoard,
        IReadOnlyList<OpeningCrewBalanceItem> CrewBalances,
        IReadOnlyList<OpeningCashItem> Cash);

    public record OpeningBalancesResponse(
        DateOnly AsOf,
        string? MasterName,
        decimal MasterAccountBalance,
        decimal CigarettesOnBoard,
        IReadOnlyList<OpeningCrewBalanceItem> CrewBalances,
        IReadOnlyList<OpeningCashItem> Cash);

    /// <summary>
    /// Master account − crew balances + cash on board + cigarettes on board should be 0
    /// (same check as the MGA "Final Summary"). All in base currency.
    /// </summary>
    public record OpeningCheckResponse(
        string BaseCurrencyCode,
        decimal MasterAccountBalance,
        decimal CrewBalancesTotal,
        decimal CashOnBoard,
        decimal CigarettesOnBoard,
        decimal Difference);
}