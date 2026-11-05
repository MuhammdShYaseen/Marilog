using Marilog.Contracts.DTOs.Responses;

namespace Marilog.Contracts.DTOs.Reports.Contract
{
    public sealed class ContractSummary
    {
        public int Id { get; init; }
        public string ContractNumber { get; init; } = null!;
        public int ContractTypeId { get; init; }
        public string Type { get; init; } = null!;          // contract type name
        public string Status { get; init; } = null!;
        public DateOnly EffectiveDate { get; init; }
        public DateOnly? ExpiryDate { get; init; }
        public int AmendmentsCount { get; init; }
        public List<ContractPartyResponse> Parties { get; init; } = [];
    }
}
