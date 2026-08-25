using Marilog.Contracts.DTOs.Requests.CreditBalance;
using Marilog.Contracts.DTOs.Responses;

namespace Marilog.Application.Interfaces.Services.CreditBalance
{
    public interface ICreditBalanceService
    {
        Task<CreditBalanceResponse> GetByIdAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<CreditBalanceResponse>> GetAllAsync(CancellationToken ct = default);
        Task<CreditBalanceResponse> CreateAsync(CreateCreditBalanceRequest request, CancellationToken ct = default);
        Task<CreditBalanceResponse> UpdateAsync(int id, UpdateCreditBalanceRequest request, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);
    }
}
