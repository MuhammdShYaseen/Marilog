using Marilog.Ship.Domain.Enums;

namespace Marilog.Ship.Services.OwnerRemittances
{
    public record OwnerRemittanceResponse(
        int Id,
        DateOnly Date,
        OwnerRemittanceType Type,
        decimal Amount,
        int CurrencyId,
        string CurrencyCode,
        int? CrewContractId,
        string? CrewName,
        int? PortId,
        string? PortName,
        string? Reference,
        string? Notes);

    /// <summary>CrewContractId only for CrewPaidAshore (the crew member's account is reduced too).</summary>
    public record SaveOwnerRemittanceRequest(
        DateOnly Date,
        OwnerRemittanceType Type,
        decimal Amount,
        int CurrencyId,
        int? CrewContractId,
        int? PortId,
        string? Reference,
        string? Notes);
}