namespace Marilog.Contracts.DTOs.Responses
{
    public sealed class ContractAmendmentResponse
    {
        public int AmendmentNumber { get; init; }
        public string Description { get; init; } = null!;
        public DateOnly EffectiveDate { get; init; }
        public string ChangedBy { get; init; } = null!;
        public DateTime RecordedAtUtc { get; init; }
    }

}
