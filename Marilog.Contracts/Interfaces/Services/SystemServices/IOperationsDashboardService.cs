using Marilog.Contracts.DTOs.Reports.DocumentReports;
using Marilog.Contracts.DTOs.Responses;


namespace Marilog.Contracts.Interfaces.Services.SystemServices
{
    public interface IOperationsDashboardService
    {
        Task<OperationsDashboardResponse>GetAsync(CancellationToken ct = default);

        // ── Dashboard Charts ─────────────────────────────────────────────────────
        Task<IReadOnlyList<FinancialChartPointResponse>> GetFinancialSummaryAsync(DocumentFilterOptions options, CancellationToken ct = default);
        Task<IReadOnlyList<VoyageChartPointResponse>> GetVoyageSummaryAsync(DateOnly from, DateOnly to, int? vesselId = null, CancellationToken ct = default);
    }
}
