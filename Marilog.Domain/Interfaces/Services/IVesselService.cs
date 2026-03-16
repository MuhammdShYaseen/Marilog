using Marilog.Domain.Entities;

namespace Marilog.Domain.Interfaces.Services
{
    public interface IVesselService
    {
        // ── Queries ───────────────────────────────────────────────────────────────
        Task<Vessel?>              GetByIdAsync(int id, CancellationToken ct = default);
        Task<Vessel?>              GetByImoAsync(string imoNumber, CancellationToken ct = default);
        Task<IReadOnlyList<Vessel>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<Vessel>> GetActiveAsync(CancellationToken ct = default);
        Task<IReadOnlyList<Vessel>> GetByCompanyAsync(int companyId, CancellationToken ct = default);

        // ── Commands ─────────────────────────────────────────────────────────────
        Task<Vessel> CreateAsync(int companyId, string vesselName,
                                 string? imoNumber = null, decimal? grossTonnage = null,
                                 int? flagCountryId = null, string? notes = null,
                                 CancellationToken ct = default);
        Task         UpdateAsync(int id, string vesselName,
                                 string? imoNumber = null, decimal? grossTonnage = null,
                                 int? flagCountryId = null, string? notes = null,
                                 CancellationToken ct = default);
        Task         ActivateAsync(int id, CancellationToken ct = default);
        Task         DeactivateAsync(int id, CancellationToken ct = default);
        Task         DeleteAsync(int id, CancellationToken ct = default);
    }
}
