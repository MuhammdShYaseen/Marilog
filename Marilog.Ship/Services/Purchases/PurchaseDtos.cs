using Marilog.Kernel.Enums;

namespace Marilog.Ship.Services.Purchases
{
    /// <summary>
    /// One purchase from the quick-entry dialog. Empty DocNumber → NR-0001, NR-0002, ...
    /// PaidWith null = not paid yet (the office pays, or it is paid later from the details).
    /// </summary>
    public record CreatePurchaseRequest(
        int SupplierId,
        int DocTypeId,
        string? DocNumber,
        DateOnly DocDate,
        int? PortId,
        int CurrencyId,
        decimal Amount,
        string? Reference,
        PaymentMethod? PaidWith);
}