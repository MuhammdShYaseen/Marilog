using Marilog.Domain.Entities;

namespace Marilog.Domain.Interfaces.Services
{
    public interface IOfficeService
    {
        // ── Queries ───────────────────────────────────────────────────────────────
        Task<Office?>              GetByIdAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<Office>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<Office>> GetActiveAsync(CancellationToken ct = default);
        Task<IReadOnlyList<Office>> GetByCountryAsync(int countryId, CancellationToken ct = default);

        // ── Commands ─────────────────────────────────────────────────────────────
        Task<Office> CreateAsync(string officeName, string city, int countryId,
                                 string? address = null, string? phone = null,
                                 string? contactName = null, CancellationToken ct = default);
        Task         UpdateAsync(int id, string officeName, string city, int countryId,
                                 string? address = null, string? phone = null,
                                 string? contactName = null, CancellationToken ct = default);
        Task         ActivateAsync(int id, CancellationToken ct = default);
        Task         DeactivateAsync(int id, CancellationToken ct = default);
        Task         DeleteAsync(int id, CancellationToken ct = default);
    }
}
