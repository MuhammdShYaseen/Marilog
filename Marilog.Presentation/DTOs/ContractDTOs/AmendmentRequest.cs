namespace Marilog.Presentation.DTOs.ContractDTOs
{
    public record AmendmentRequest(string Description, DateOnly EffectiveDate, string ChangedBy);
}
