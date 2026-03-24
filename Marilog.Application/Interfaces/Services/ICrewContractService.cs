using Marilog.Application.DTOs;
using Marilog.Domain.Entities;

namespace Marilog.Application.Interfaces.Services
{
    public interface ICrewContractService
    {
        // ── Queries ───────────────────────────────────────────────────────────────
        Task<CrewContractResponse?>              GetByIdAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<CrewContractResponse>> GetByPersonAsync(int personId, CancellationToken ct = default);
        Task<IReadOnlyList<CrewContractResponse>> GetByVesselAsync(int vesselId, CancellationToken ct = default);
        Task<IReadOnlyList<CrewContractResponse>> GetActiveByVesselAsync(int vesselId, CancellationToken ct = default);
        Task<CrewContractResponse?>              GetActiveMasterAsync(int vesselId, CancellationToken ct = default);
        Task<IReadOnlyList<CrewContractResponse>> GetActiveOnDateAsync(DateOnly date, CancellationToken ct = default);

        // ── Commands ─────────────────────────────────────────────────────────────
        Task<CrewContractResponse> CreateAsync(int personId, int vesselId, int rankId,
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
