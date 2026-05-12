

namespace Marilog.Contracts.DTOs.Requests.ContractDTOs
{
    public record UpdateContractRequest(int Id, string ContractNumber, string Type, DateOnly EffectiveDate, DateOnly? ExpiryDate, string? Notes);
}

