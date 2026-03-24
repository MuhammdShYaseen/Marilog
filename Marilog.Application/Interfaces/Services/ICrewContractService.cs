using Marilog.Domain.Entities;

namespace Marilog.Application.Interfaces.Services
{
    public interface ICrewContractService
    {
        // ── Queries ───────────────────────────────────────────────────────────────
        Task<CrewContract?>              GetByIdAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<CrewContract>> GetByPersonAsync(int personId, CancellationToken ct = default);
        Task<IReadOnlyList<CrewContract>> GetByVesselAsync(int vesselId, CancellationToken ct = default);
        Task<IReadOnlyList<CrewContract>> GetActiveByVesselAsync(int vesselId, CancellationToken ct = default);
        Task<CrewContract?>              GetActiveMasterAsync(int vesselId, CancellationToken ct = default);
        Task<IReadOnlyList<CrewContract>> GetActiveOnDateAsync(DateOnly date, CancellationToken ct = default);

        // ── Commands ─────────────────────────────────────────────────────────────
        Task<CrewContract> CreateAsync(int personId, int vesselId, int rankId,
                                       decimal monthlyWage, DateOnly signOnDate,
                                       int? signOnPort = null, string? notes = null,
                                       CancellationToken ct = default);
        Task               UpdateAsync(int id, decimal monthlyWage, string? notes = null,
                                       CancellationToken ct = default);
        Task               SignOffAsync(int id, DateOnly signOffDate, int? signOffPort = null,
                                        CancellationToken ct = default);
        Task               DeleteAsync(int id, CancellationToken ct = default);
    }
}
