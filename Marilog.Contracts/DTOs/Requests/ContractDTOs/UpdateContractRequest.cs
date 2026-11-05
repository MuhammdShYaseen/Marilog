namespace Marilog.Contracts.DTOs.Requests.ContractDTOs
{
    public record UpdateContractRequest(int Id, string ContractNumber, int ContractTypeId, DateOnly EffectiveDate, DateOnly? ExpiryDate, string? Notes);
}
