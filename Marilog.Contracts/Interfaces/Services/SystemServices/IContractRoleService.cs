using Marilog.Contracts.DTOs.Responses;

namespace Marilog.Contracts.Interfaces.Services.SystemServices
{
    public interface IContractRoleService
    {
        Task<IReadOnlyList<ContractLookupResponse>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<ContractLookupResponse>> GetActiveAsync(CancellationToken ct = default);
        Task<ContractLookupResponse?> GetByIdAsync(int id, CancellationToken ct = default);

        Task<ContractLookupResponse> CreateAsync(string code, string name, int sortOrder = 0, CancellationToken ct = default);
        Task UpdateAsync(int id, string name, int sortOrder, CancellationToken ct = default);
        Task ActivateAsync(int id, CancellationToken ct = default);
        Task DeactivateAsync(int id, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);
    }
}
