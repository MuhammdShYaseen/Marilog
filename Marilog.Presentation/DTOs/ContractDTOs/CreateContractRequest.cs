namespace Marilog.Presentation.DTOs.ContractDTOs
{
    public record CreateContractRequest(string ContractNumber, string Type, DateOnly EffectiveDate, DateOnly? ExpiryDate, string? Notes);
}
