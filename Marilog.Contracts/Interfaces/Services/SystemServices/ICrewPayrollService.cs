using Marilog.Contracts.DTOs.Reports.CrewPayrollReports;
using Marilog.Contracts.DTOs.Requests.CrewPayrollDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Kernel.Enums;


namespace Marilog.Contracts.Interfaces.Services.SystemServices
{
    public interface ICrewPayrollService
    {
        // ── Queries ───────────────────────────────────────────────────────────────
        Task<CrewPayrollResponse?>              GetByIdAsync(int id, CancellationToken ct = default);
        Task<CrewPayrollResponse?>              GetWithDisbursementsAsync(int id, CancellationToken ct = default);
        Task<CrewPayrollResponse?>              GetByContractAndMonthAsync(int contractId, DateOnly month, CancellationToken ct = default);
        Task<IReadOnlyList<CrewPayrollResponse>> GetByContractAsync(int contractId, CancellationToken ct = default);
        Task<IReadOnlyList<CrewPayrollResponse>> GetByMonthAsync(DateOnly month, CancellationToken ct = default);
        Task<IReadOnlyList<CrewPayrollResponse>> GetByStatusAsync(PayrollStatus status, CancellationToken ct = default);
        Task<IReadOnlyList<CrewPayrollResponse>> GetOutstandingAsync(CancellationToken ct = default);

        // ── Commands ─────────────────────────────────────────────────────────────
        Task<CrewPayrollResponse> CreateAsync(int contractId, DateOnly payrollMonth, int currencyId,
     decimal allowances = 0m, decimal deductions = 0m, string? notes = null,
     CancellationToken ct = default);

        Task UpdateAsync(int id, int contractId, int currencyId, decimal allowances = 0m,
                         decimal deductions = 0m, string? notes = null, CancellationToken ct = default);
        Task<IReadOnlyList<CrewPayrollResponse>> CreateRangeAsync(IEnumerable<CreateCrewPayrollRequest> commands,
                                                 CancellationToken ct = default);
        Task              ApproveAsync(int id, CancellationToken ct = default);
        Task              CancelAsync(int id, string reason, CancellationToken ct = default);
        Task              DeleteAsync(int id, CancellationToken ct = default);

        // ── Disbursements ─────────────────────────────────────────────────────────
        Task<DisbursementResponse> PayAsync(int payrollId, PayCrewPayrollRequest request, CancellationToken ct = default);
        Task                          ConfirmDisbursementAsync(int payrollId, int disbursementId,
                                                               CancellationToken ct = default);
        Task                          CancelDisbursementAsync(int payrollId, int disbursementId,
                                                              string reason,
                                                              CancellationToken ct = default);

        //----Reports--------------------------------------------------------------------------------
        Task<CrewPayrollReport> GetCrewPayrollReportAsync(CrewPayrollFilterOptions options,
        CancellationToken ct = default);
    }
}
