using Marilog.Domain.Entities;

namespace Marilog.Domain.Interfaces.Services
{
    public interface ICrewPayrollService
    {
        // ── Queries ───────────────────────────────────────────────────────────────
        Task<CrewPayroll?>              GetByIdAsync(int id, CancellationToken ct = default);
        Task<CrewPayroll?>              GetWithDisbursementsAsync(int id, CancellationToken ct = default);
        Task<CrewPayroll?>              GetByContractAndMonthAsync(int contractId, DateOnly month, CancellationToken ct = default);
        Task<IReadOnlyList<CrewPayroll>> GetByContractAsync(int contractId, CancellationToken ct = default);
        Task<IReadOnlyList<CrewPayroll>> GetByMonthAsync(DateOnly month, CancellationToken ct = default);
        Task<IReadOnlyList<CrewPayroll>> GetByStatusAsync(PayrollStatus status, CancellationToken ct = default);
        Task<IReadOnlyList<CrewPayroll>> GetOutstandingAsync(CancellationToken ct = default);

        // ── Commands ─────────────────────────────────────────────────────────────
        Task<CrewPayroll> CreateAsync(int contractId, DateOnly payrollMonth,
                                      int workingDays, decimal basicWage,
                                      decimal allowances = 0m, decimal deductions = 0m,
                                      string? notes = null, CancellationToken ct = default);
        Task              UpdateAsync(int id, int workingDays, decimal basicWage,
                                      decimal allowances = 0m, decimal deductions = 0m,
                                      string? notes = null, CancellationToken ct = default);
        Task              ApproveAsync(int id, CancellationToken ct = default);
        Task              CancelAsync(int id, string reason, CancellationToken ct = default);
        Task              DeleteAsync(int id, CancellationToken ct = default);

        // ── Disbursements ─────────────────────────────────────────────────────────
        Task<CrewPayrollDisbursement> PayCashAsync(int payrollId, int voyageId,
                                                   decimal amount, DateOnly paidOn,
                                                   string? notes = null,
                                                   CancellationToken ct = default);
        Task<CrewPayrollDisbursement> PayAtOfficeAsync(int payrollId, int officeId,
                                                       decimal amount, DateOnly paidOn,
                                                       string recipientName,
                                                       string recipientIdNumber,
                                                       string? notes = null,
                                                       CancellationToken ct = default);
        Task<CrewPayrollDisbursement> PayByBankTransferAsync(int payrollId, int swiftTransferId,
                                                             decimal amount, DateOnly paidOn,
                                                             string? notes = null,
                                                             CancellationToken ct = default);
        Task                          ConfirmDisbursementAsync(int payrollId, int disbursementId,
                                                               CancellationToken ct = default);
        Task                          CancelDisbursementAsync(int payrollId, int disbursementId,
                                                              string reason,
                                                              CancellationToken ct = default);
    }
}
