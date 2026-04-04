

namespace Marilog.Application.DTOs.Responses
{
    public sealed class ContractDetailResponse
    {

        public int Id { get; init; }
        public string ContractNumber { get; init; } = null!;
        public string Type { get; init; } = null!;
        public string Status { get; init; } = null!;
        public DateOnly EffectiveDate { get; init; }
        public DateOnly? ExpiryDate { get; init; }
        public string? Notes { get; init; }
        public string? ContractFileUrl { get; init; }
        public string? ContractFileName { get; init; }

        public List<ContractPartyResponse> Parties { get; init; } = [];
        public List<ContractAmendmentResponse> Amendments { get; init; } = [];
    }
}
