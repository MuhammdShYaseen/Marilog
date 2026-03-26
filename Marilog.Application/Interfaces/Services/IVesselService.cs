using Marilog.Application.DTOs;
using Marilog.Domain.Entities;

namespace Marilog.Application.Interfaces.Services
{
    public interface IVesselService
    {
        // ── Queries ───────────────────────────────────────────────────────────────
        Task<VesselResponse?>              GetByIdAsync(int id, CancellationToken ct = default);
        Task<VesselResponse?>              GetByImoAsync(string imoNumber, CancellationToken ct = default);
        Task<IReadOnlyList<VesselResponse>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<VesselResponse>> GetActiveAsync(CancellationToken ct = default);
        Task<IReadOnlyList<VesselResponse>> GetByCompanyAsync(int companyId, CancellationToken ct = default);

        // ── Commands ─────────────────────────────────────────────────────────────
        Task<VesselResponse> CreateAsync(int companyId, string vesselName,
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
