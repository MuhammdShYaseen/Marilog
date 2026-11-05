namespace Marilog.Contracts.DTOs.Requests.ContractDTOs
{
    public record CreateContractRequest(string ContractNumber, int ContractTypeId, DateOnly EffectiveDate, DateOnly? ExpiryDate, string? Notes);
}
