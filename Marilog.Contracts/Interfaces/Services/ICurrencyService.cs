using Marilog.Contracts.DTOs.Requests.CurrencyDTOs;
using Marilog.Contracts.DTOs.Responses;

namespace Marilog.Contracts.Interfaces.Services
{
    public interface ICurrencyService
    {
        // ── Queries ───────────────────────────────────────────────────────────────
        Task<CurrencyResponse?>              GetByIdAsync(int id, CancellationToken ct = default);
        Task<CurrencyResponse?>              GetByCodeAsync(string code, CancellationToken ct = default);
        Task<CurrencyResponse?>              GetBaseCurrencyAsync(CancellationToken ct = default);
        Task<IReadOnlyList<CurrencyResponse>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<CurrencyResponse>> GetActiveAsync(CancellationToken ct = default);

        // ── Commands ─────────────────────────────────────────────────────────────
        Task<CurrencyResponse> CreateAsync(string code, string name, decimal exchangeRate, string? symbol = null, CancellationToken ct = default);
        Task<IReadOnlyList<CurrencyResponse>> CreateRangeAsync(IEnumerable<CreateCurrencyRequest> commands, CancellationToken ct = default);
        Task           UpdateAsync(int id, string name, decimal exchangeRate, string? symbol = null, CancellationToken ct = default);
        Task           UpdateRateAsync(int id, decimal newRate, CancellationToken ct = default);
        Task           SetAsBaseAsync(int id, CancellationToken ct = default);
        Task           ActivateAsync(int id, CancellationToken ct = default);
        Task           DeactivateAsync(int id, CancellationToken ct = default);
        Task           DeleteAsync(int id, CancellationToken ct = default);
    }
}
