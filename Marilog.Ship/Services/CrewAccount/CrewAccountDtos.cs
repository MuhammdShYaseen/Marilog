using Marilog.Ship.Domain.Enums;

namespace Marilog.Ship.Services.CrewAccount
{
    public record CrewWithdrawalResponse(
        int Id,
        DateOnly Date,
        int CrewContractId,
        string CrewName,
        string RankName,
        CrewWithdrawalType Type,
        int? DeductionTypeId,
        string? DeductionTypeName,
        decimal Amount,
        int CurrencyId,
        string CurrencyCode,
        string? Reason);

    /// <summary>DeductionTypeId and Reason are required for Other only.</summary>
    public record SaveCrewWithdrawalRequest(
        DateOnly Date,
        int CrewContractId,
        CrewWithdrawalType Type,
        int? DeductionTypeId,
        decimal Amount,
        int CurrencyId,
        string? Reason);

    public record CrewExtraResponse(
        int Id,
        DateOnly Date,
        int CrewContractId,
        string CrewName,
        string RankName,
        CrewExtraType Type,
        decimal Amount,
        int CurrencyId,
        string CurrencyCode,
        string Reason);

    public record SaveCrewExtraRequest(
        DateOnly Date,
        int CrewContractId,
        CrewExtraType Type,
        decimal Amount,
        int CurrencyId,
        string Reason);
}