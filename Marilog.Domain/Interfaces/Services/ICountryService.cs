using Marilog.Domain.Entities;

namespace Marilog.Domain.Interfaces.Services
{
    public interface ICountryService
    {
        // ── Queries ───────────────────────────────────────────────────────────────
        Task<Country?>              GetByIdAsync(int id, CancellationToken ct = default);
        Task<Country?>              GetByCodeAsync(string code, CancellationToken ct = default);
        Task<IReadOnlyList<Country>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<Country>> GetActiveAsync(CancellationToken ct = default);
        Task<bool>                  ExistsByCodeAsync(string code, CancellationToken ct = default);

        // ── Commands ─────────────────────────────────────────────────────────────
        Task<Country> CreateAsync(string countryCode, string countryName, CancellationToken ct = default);
        Task          UpdateAsync(int id, string countryCode, string countryName, CancellationToken ct = default);
        Task          ActivateAsync(int id, CancellationToken ct = default);
        Task          DeactivateAsync(int id, CancellationToken ct = default);
        Task          DeleteAsync(int id, CancellationToken ct = default);
    }
}
