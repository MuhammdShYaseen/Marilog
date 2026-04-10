namespace Marilog.Contracts.DTOs.Requests.ContractDTOs
{
    public record CreateContractRequest(string ContractNumber, string Type, DateOnly EffectiveDate, DateOnly? ExpiryDate, string? Notes);
}
