namespace Marilog.Contracts.DTOs.Requests.ContractDTOs
{
    public record AmendmentRequest(string Description, DateOnly EffectiveDate, string ChangedBy);
}
