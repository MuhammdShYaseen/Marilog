using Marilog.Domain.Entities;

namespace Marilog.Domain.Interfaces.Services
{
    public interface ICurrencyService
    {
        // ── Queries ───────────────────────────────────────────────────────────────
        Task<Currency?>              GetByIdAsync(int id, CancellationToken ct = default);
        Task<Currency?>              GetByCodeAsync(string code, CancellationToken ct = default);
        Task<Currency?>              GetBaseCurrencyAsync(CancellationToken ct = default);
        Task<IReadOnlyList<Currency>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<Currency>> GetActiveAsync(CancellationToken ct = default);

        // ── Commands ─────────────────────────────────────────────────────────────
        Task<Currency> CreateAsync(string code, string name, decimal exchangeRate, string? symbol = null, CancellationToken ct = default);
        Task           UpdateAsync(int id, string name, decimal exchangeRate, string? symbol = null, CancellationToken ct = default);
        Task           UpdateRateAsync(int id, decimal newRate, CancellationToken ct = default);
        Task           SetAsBaseAsync(int id, CancellationToken ct = default);
        Task           ActivateAsync(int id, CancellationToken ct = default);
        Task           DeactivateAsync(int id, CancellationToken ct = default);
        Task           DeleteAsync(int id, CancellationToken ct = default);
    }
}
