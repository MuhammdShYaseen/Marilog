using Marilog.Domain.Entities;

namespace Marilog.Application.Interfaces.Services
{
    public interface IPortService
    {
        // ── Queries ───────────────────────────────────────────────────────────────
        Task<Port?>              GetByIdAsync(int id, CancellationToken ct = default);
        Task<Port?>              GetByCodeAsync(string code, CancellationToken ct = default);
        Task<IReadOnlyList<Port>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<Port>> GetActiveAsync(CancellationToken ct = default);
        Task<IReadOnlyList<Port>> GetByCountryAsync(int countryId, CancellationToken ct = default);
        Task<bool>               ExistsByCodeAsync(string code, CancellationToken ct = default);

        // ── Commands ─────────────────────────────────────────────────────────────
        Task<Port> CreateAsync(string portCode, string portName, int? countryId = null, CancellationToken ct = default);
        Task       UpdateAsync(int id, string portCode, string portName, int? countryId = null, CancellationToken ct = default);
        Task       ActivateAsync(int id, CancellationToken ct = default);
        Task       DeactivateAsync(int id, CancellationToken ct = default);
        Task       DeleteAsync(int id, CancellationToken ct = default);
    }
}
