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


    public record WagesPaidAshoreItem(int CrewContractId, decimal Amount);

    /// <summary>
    /// One payment by the office for several crew (e.g. "CREW WAGES PAID ASHORE AUG 2026").
    /// Saved as one CrewPaidAshore remittance per crew member; page 4 shows them as one line.
    /// </summary>
    public record SaveWagesPaidAshoreRequest(
        DateOnly Date,
        int CurrencyId,
        string Label,
        string? Reference,
        IReadOnlyList<WagesPaidAshoreItem> Items);
}