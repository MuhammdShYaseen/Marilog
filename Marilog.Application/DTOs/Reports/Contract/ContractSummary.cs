

namespace Marilog.Application.DTOs.Reports.Contract
{
    public sealed class ContractSummary
    {
        public int Id { get; init; }
        public string ContractNumber { get; init; } = null!;
        public string Type { get; init; } = null!;
        public string Status { get; init; } = null!;
        public DateOnly EffectiveDate { get; init; }
        public DateOnly? ExpiryDate { get; init; }
        public int PartiesCount { get; init; }
        public int AmendmentsCount { get; init; }
    }
}
