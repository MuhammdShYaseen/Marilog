using Marilog.Application.DTOs.Commands.CrewContract;
using Marilog.Application.DTOs.Reports.CrewContractReports;
using Marilog.Application.DTOs.Responses;
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
        Task<CrewContractResponse> CreateAsync(int durationInMonth, int personId, int vesselId, int rankId,
                                       decimal monthlyWage, DateOnly signOnDate,
                                       int? signOnPort = null, string? notes = null,
                                       CancellationToken ct = default);

        Task<IReadOnlyList<CrewContractResponse>> CreateRangeAsync(
                                                IEnumerable<CreateCrewContractCommand> commands,
                                                CancellationToken ct = default);
        Task               UpdateAsync(int id, decimal monthlyWage, string? notes = null,
                                       CancellationToken ct = default);
        Task               SignOffAsync(int id, DateOnly signOffDate, int? signOffPort = null,
                                        CancellationToken ct = default);
        Task               DeleteAsync(int id, CancellationToken ct = default);

        //----Reports-----------------------------------------------------------------
        Task<CrewContractReport> GetCrewContractsReportAsync(CrewContractFilterOptions options, CancellationToken ct = default);
        Task<IReadOnlyList<CrewContractResponse>> GetExpiredAsync(CancellationToken ct);
        Task<IReadOnlyList<CrewContractResponse>> GetAboutExpireAsync(CancellationToken ct);
    }
}
