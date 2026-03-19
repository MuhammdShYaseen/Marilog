using Microsoft.EntityFrameworkCore;
using Marilog.Domain.Entities;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Domain.Interfaces.Services;

namespace Marilog.Application.Services
{
    public class CrewPayrollService : ICrewPayrollService
    {
        private readonly IRepository<CrewPayroll>   _repo;
        private readonly IRepository<CrewContract>  _contractRepo;
        private readonly IRepository<Voyage>        _voyageRepo;
        private readonly IRepository<SwiftTransfer> _swiftRepo;
        private readonly IRepository<Office>        _officeRepo;

        public CrewPayrollService(
            IRepository<CrewPayroll>   repo,
            IRepository<CrewContract>  contractRepo,
            IRepository<Voyage>        voyageRepo,
            IRepository<SwiftTransfer> swiftRepo,
            IRepository<Office>        officeRepo)
        {
            _repo         = repo;
            _contractRepo = contractRepo;
            _voyageRepo   = voyageRepo;
            _swiftRepo    = swiftRepo;
            _officeRepo   = officeRepo;
        }

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<CrewPayroll?> GetByIdAsync(int id, CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .Include(x => x.Contract).ThenInclude(x => x.Person)
                          .Include(x => x.Contract).ThenInclude(x => x.Rank)
                          .FirstOrDefaultAsync(x => x.Id == id, ct);

        public async Task<CrewPayroll?> GetWithDisbursementsAsync(int id,
            CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .Include(x => x.Disbursements).ThenInclude(x => x.Voyage)
                          .Include(x => x.Disbursements).ThenInclude(x => x.Office)
                          .Include(x => x.Disbursements).ThenInclude(x => x.SwiftTransfer)
                          .Include(x => x.Contract).ThenInclude(x => x.Person)
                          .Include(x => x.Contract).ThenInclude(x => x.Rank)
                          .FirstOrDefaultAsync(x => x.Id == id, ct);

        public async Task<CrewPayroll?> GetByContractAndMonthAsync(int contractId,
            DateOnly month, CancellationToken ct = default)
        {
            var firstDay = new DateOnly(month.Year, month.Month, 1);
            return await _repo.Query().AsNoTracking()
                              .Include(x => x.Disbursements)
                              .FirstOrDefaultAsync(x => x.ContractId   == contractId &&
                                                        x.PayrollMonth == firstDay, ct);
        }

        public async Task<IReadOnlyList<CrewPayroll>> GetByContractAsync(int contractId,
            CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .Where(x => x.ContractId == contractId)
                          .Include(x => x.Disbursements)
                          .OrderByDescending(x => x.PayrollMonth)
                          .ToListAsync(ct);

        public async Task<IReadOnlyList<CrewPayroll>> GetByMonthAsync(DateOnly month,
            CancellationToken ct = default)
        {
            var firstDay = new DateOnly(month.Year, month.Month, 1);
            return await _repo.Query().AsNoTracking()
                              .Where(x => x.PayrollMonth == firstDay)
                              .Include(x => x.Contract).ThenInclude(x => x.Person)
                              .Include(x => x.Contract).ThenInclude(x => x.Vessel)
                              .Include(x => x.Contract).ThenInclude(x => x.Rank)
                              .OrderBy(x => x.Contract.Person.FullName)
                              .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<CrewPayroll>> GetByStatusAsync(PayrollStatus status,
            CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .Where(x => x.Status == status)
                          .Include(x => x.Contract).ThenInclude(x => x.Person)
                          .Include(x => x.Contract).ThenInclude(x => x.Rank)
                          .OrderByDescending(x => x.PayrollMonth)
                          .ToListAsync(ct);

        public async Task<IReadOnlyList<CrewPayroll>> GetOutstandingAsync(
            CancellationToken ct = default)
            => await _repo.Query()
                          .Where(x => x.Status == PayrollStatus.Approved ||
                                      x.Status == PayrollStatus.PartiallyPaid)
                          .Include(x => x.Disbursements)
                          .Include(x => x.Contract).ThenInclude(x => x.Person)
                          .Include(x => x.Contract).ThenInclude(x => x.Vessel)
                          .OrderBy(x => x.PayrollMonth)
                          .ThenBy(x => x.Contract.Person.FullName)
                          .ToListAsync(ct);

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<CrewPayroll> CreateAsync(int contractId, DateOnly payrollMonth,
            int workingDays, decimal basicWage, decimal allowances = 0m,
            decimal deductions = 0m, string? notes = null,
            CancellationToken ct = default)
        {
            await EnsureContractActiveAsync(contractId, ct);
            await EnsureNoDuplicatePayrollAsync(contractId, payrollMonth, excludeId: null, ct);

            var payroll = CrewPayroll.Create(contractId, payrollMonth, workingDays,
                                             basicWage, allowances, deductions, notes);
            await _repo.AddAsync(payroll, ct);
            await _repo.SaveChangesAsync(ct);
            return payroll;
        }

        public async Task UpdateAsync(int id, int workingDays, decimal basicWage,
            decimal allowances = 0m, decimal deductions = 0m,
            string? notes = null, CancellationToken ct = default)
        {
            var payroll = await GetOrThrowAsync(id, ct);
            payroll.Update(workingDays, basicWage, allowances, deductions, notes);
            _repo.Update(payroll);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task ApproveAsync(int id, CancellationToken ct = default)
        {
            var payroll = await GetOrThrowAsync(id, ct);
            payroll.Approve();
            _repo.Update(payroll);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task CancelAsync(int id, string reason, CancellationToken ct = default)
        {
            var payroll = await GetOrThrowAsync(id, ct);
            payroll.Cancel(reason);
            _repo.Update(payroll);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var payroll = await _repo.Query()
                .Include(x => x.Disbursements)
                .FirstOrDefaultAsync(x => x.Id == id, ct)
                ?? throw new KeyNotFoundException($"CrewPayroll {id} not found.");

            if (payroll.Status != PayrollStatus.Draft)
                throw new InvalidOperationException(
                    "Only Draft payrolls can be deleted.");

            _repo.HardDelete(payroll);
            await _repo.SaveChangesAsync(ct);
        }

        // ── Disbursements ─────────────────────────────────────────────────────────

        public async Task<CrewPayrollDisbursement> PayCashAsync(int payrollId, int voyageId,
            decimal amount, DateOnly paidOn, string? notes = null,
            CancellationToken ct = default)
        {
            var payroll = await GetWithDisbursementsOrThrowAsync(payrollId, ct);
            await EnsureVoyageExistsAsync(voyageId, ct);

            var disbursement = payroll.PayCash(voyageId, amount, paidOn, notes);
            _repo.Update(payroll);
            await _repo.SaveChangesAsync(ct);
            return disbursement;
        }

        public async Task<CrewPayrollDisbursement> PayAtOfficeAsync(int payrollId, int officeId,
            decimal amount, DateOnly paidOn, string recipientName,
            string recipientIdNumber, string? notes = null,
            CancellationToken ct = default)
        {
            var payroll = await GetWithDisbursementsOrThrowAsync(payrollId, ct);
            await EnsureOfficeExistsAsync(officeId, ct);

            var disbursement = payroll.PayAtOffice(officeId, amount, paidOn,
                                                   recipientName, recipientIdNumber, notes);
            _repo.Update(payroll);
            await _repo.SaveChangesAsync(ct);
            return disbursement;
        }

        public async Task<CrewPayrollDisbursement> PayByBankTransferAsync(int payrollId,
            int swiftTransferId, decimal amount, DateOnly paidOn,
            string? notes = null, CancellationToken ct = default)
        {
            var payroll = await GetWithDisbursementsOrThrowAsync(payrollId, ct);
            await EnsureSwiftTransferExistsAsync(swiftTransferId, ct);

            var disbursement = payroll.PayByBankTransfer(swiftTransferId, amount, paidOn, notes);
            _repo.Update(payroll);
            await _repo.SaveChangesAsync(ct);
            return disbursement;
        }

        public async Task ConfirmDisbursementAsync(int payrollId, int disbursementId,
            CancellationToken ct = default)
        {
            var payroll = await GetWithDisbursementsOrThrowAsync(payrollId, ct);
            payroll.ConfirmDisbursement(disbursementId);
            _repo.Update(payroll);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task CancelDisbursementAsync(int payrollId, int disbursementId,
            string reason, CancellationToken ct = default)
        {
            var payroll = await GetWithDisbursementsOrThrowAsync(payrollId, ct);
            payroll.CancelDisbursement(disbursementId, reason);
            _repo.Update(payroll);
            await _repo.SaveChangesAsync(ct);
        }

        // ── Private ───────────────────────────────────────────────────────────────

        private async Task<CrewPayroll> GetOrThrowAsync(int id, CancellationToken ct)
            => await _repo.GetByIdAsync(id, ct)
               ?? throw new KeyNotFoundException($"CrewPayroll {id} not found.");

        private async Task<CrewPayroll> GetWithDisbursementsOrThrowAsync(int id,
            CancellationToken ct)
            => await _repo.Query()
                          .Include(x => x.Disbursements)
                          .FirstOrDefaultAsync(x => x.Id == id, ct)
               ?? throw new KeyNotFoundException($"CrewPayroll {id} not found.");

        private async Task EnsureContractActiveAsync(int contractId, CancellationToken ct)
        {
            var exists = await _contractRepo.Query()
                .AnyAsync(x => x.ContractID == contractId && x.IsActive, ct);
            if (!exists)
                throw new KeyNotFoundException(
                    $"CrewContract {contractId} not found or inactive.");
        }

        private async Task EnsureNoDuplicatePayrollAsync(int contractId, DateOnly month,
            int? excludeId, CancellationToken ct)
        {
            var firstDay = new DateOnly(month.Year, month.Month, 1);
            var conflict = await _repo.Query()
                .AnyAsync(x => x.ContractId   == contractId    &&
                               x.PayrollMonth == firstDay       &&
                               (excludeId == null || x.Id != excludeId), ct);
            if (conflict)
                throw new InvalidOperationException(
                    $"A payroll already exists for contract {contractId} in {firstDay:yyyy-MM}.");
        }

        private async Task EnsureVoyageExistsAsync(int voyageId, CancellationToken ct)
        {
            var exists = await _voyageRepo.Query()
                .AnyAsync(x => x.VoyageID == voyageId, ct);
            if (!exists)
                throw new KeyNotFoundException($"Voyage {voyageId} not found.");
        }

        private async Task EnsureOfficeExistsAsync(int officeId, CancellationToken ct)
        {
            var exists = await _officeRepo.Query()
                .AnyAsync(x => x.Id == officeId && x.IsActive, ct);
            if (!exists)
                throw new KeyNotFoundException($"Office {officeId} not found or inactive.");
        }

        private async Task EnsureSwiftTransferExistsAsync(int swiftTransferId,
            CancellationToken ct)
        {
            var exists = await _swiftRepo.Query()
                .AnyAsync(x => x.Id == swiftTransferId && x.IsActive, ct);
            if (!exists)
                throw new KeyNotFoundException(
                    $"SwiftTransfer {swiftTransferId} not found or inactive.");
        }
    }
}
