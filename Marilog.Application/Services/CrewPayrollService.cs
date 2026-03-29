using Marilog.Application.DTOs;
using Marilog.Application.DTOs.Responses;
using Marilog.Application.Interfaces.Services;
using Marilog.Domain.Entities;
using Marilog.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Contracts;

namespace Marilog.Application.Services
{
    public class CrewPayrollService : ICrewPayrollService
    {
        private readonly IRepository<CrewPayroll>   _repo;
        private readonly IRepository<CrewContract>  _contractRepo;
        private readonly IRepository<Voyage>        _voyageRepo;
        private readonly IRepository<SwiftTransfer> _swiftRepo;
        private readonly IRepository<Office>        _officeRepo;
        private readonly IPayrollCalculatorService _CalculatorService;
        public CrewPayrollService(
            IRepository<CrewPayroll>   repo,
            IRepository<CrewContract>  contractRepo,
            IRepository<Voyage>        voyageRepo,
            IRepository<SwiftTransfer> swiftRepo,
            IRepository<Office>        officeRepo,
            IPayrollCalculatorService payrollCalculator)
        {
            _repo         = repo;
            _contractRepo = contractRepo;
            _voyageRepo   = voyageRepo;
            _swiftRepo    = swiftRepo;
            _officeRepo   = officeRepo;
            _CalculatorService = payrollCalculator;
        }

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<CrewPayrollResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new CrewPayrollResponse
                {
                    Id = x.Id,
                    ContractId = x.ContractId,
                    PersonId = x.Contract.PersonID,
                    PersonFullName = x.Contract.Person.FullName,
                    VesselId = x.Contract.VesselID,
                    VesselName = x.Contract.Vessel.VesselName,
                    PayrollMonth = x.PayrollMonth,
                    WorkingDays = x.WorkingDays,
                    BasicWage = x.BasicWage,
                    Allowances = x.Allowances,
                    Deductions = x.Deductions,
                    GrossAmount = x.GrossAmount,
                    TotalDisbursed = x.TotalDisbursed,
                    RemainingBalance = x.RemainingBalance,
                    IsFullyPaid = x.IsFullyPaid,
                    Status = x.Status,
                    Notes = x.Notes
                })
                .FirstOrDefaultAsync(ct);
        }

        public async Task<CrewPayrollResponse?> GetWithDisbursementsAsync(int id,
            CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new CrewPayrollResponse
                    {
                        Id = x.Id,
                        ContractId = x.ContractId,
                        PersonId = x.Contract.PersonID,
                        PersonFullName = x.Contract.Person.FullName,
                        VesselId = x.Contract.VesselID,
                        VesselName = x.Contract.Vessel.VesselName,
                        PayrollMonth = x.PayrollMonth,
                        WorkingDays = x.WorkingDays,
                        BasicWage = x.BasicWage,
                        Allowances = x.Allowances,
                        Deductions = x.Deductions,
                        GrossAmount = x.GrossAmount,
                        TotalDisbursed = x.TotalDisbursed,
                        RemainingBalance = x.RemainingBalance,
                        IsFullyPaid = x.IsFullyPaid,
                        Status = x.Status,
                        Notes = x.Notes,
                        Disbursements = x.Disbursements
                .Select(d => new DisbursementResponse
                {
                        Id = d.Id,
                        Amount = d.Amount,
                        Status = d.Status,
                        VoyageId = d.Voyage!.Id,
                        OfficeName = d.Office!.OfficeName,
                        SwiftReference = d.SwiftTransfer!.SwiftReference,
                        CancelReason = d.CancelReason,
                        SwiftTransferId = d.SwiftTransferId,
                        Method = d.Method,
                        Notes = d.Notes,
                        OfficeId = d.OfficeId,
                        PaidOn = d.PaidOn,
                        RecipientIdNumber = d.RecipientIdNumber,
                        RecipientName = d.RecipientName,
                        VoyageNumber = d.Voyage.VoyageNumber
                })
                .ToList()
        })
        .FirstOrDefaultAsync(ct);
}
        public async Task<CrewPayrollResponse?> GetByContractAndMonthAsync(int contractId,
            DateOnly month, CancellationToken ct = default)
        {
            var firstDay = new DateOnly(month.Year, month.Month, 1);

            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.ContractId == contractId && x.PayrollMonth == firstDay)
                .Select(x => new CrewPayrollResponse
                {
                    Id = x.Id,
                    ContractId = x.ContractId,
                    PersonId = x.Contract.PersonID,
                    PersonFullName = x.Contract.Person.FullName,
                    VesselId = x.Contract.VesselID,
                    VesselName = x.Contract.Vessel.VesselName,
                    PayrollMonth = x.PayrollMonth,
                    TotalDisbursed = x.TotalDisbursed,
                    RemainingBalance = x.RemainingBalance,
                    Status = x.Status,

                    Disbursements = x.Disbursements
                        .Select(d => new DisbursementResponse
                        {
                            Id = d.Id,
                            Amount = d.Amount
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync(ct);
        }

        public async Task<IReadOnlyList<CrewPayrollResponse>> GetByContractAsync(int contractId,
            CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.ContractId == contractId)
                .OrderByDescending(x => x.PayrollMonth)
                .Select(x => new CrewPayrollResponse
                {
                    Id = x.Id,
                    PayrollMonth = x.PayrollMonth,
                    TotalDisbursed = x.TotalDisbursed,
                    RemainingBalance = x.RemainingBalance,
                    Status = x.Status
                })
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<CrewPayrollResponse>> GetByMonthAsync(DateOnly month,
            CancellationToken ct = default)
        {
            var firstDay = new DateOnly(month.Year, month.Month, 1);

            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.PayrollMonth == firstDay)
                .OrderBy(x => x.Contract.Person.FullName)
                .Select(x => new CrewPayrollResponse
                {
                    Id = x.Id,
                    PersonFullName = x.Contract.Person.FullName,
                    VesselName = x.Contract.Vessel.VesselName,
                    PayrollMonth = x.PayrollMonth,
                    Status = x.Status
                })
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<CrewPayrollResponse>> GetByStatusAsync(PayrollStatus status,
            CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.Status == status)
                .OrderByDescending(x => x.PayrollMonth)
                .Select(x => new CrewPayrollResponse
                {
                    Id = x.Id,
                    PersonFullName = x.Contract.Person.FullName,
                    PayrollMonth = x.PayrollMonth,
                    Status = x.Status
                })
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<CrewPayrollResponse>> GetOutstandingAsync(
            CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.Status == PayrollStatus.Approved ||
                            x.Status == PayrollStatus.PartiallyPaid)
                .OrderBy(x => x.PayrollMonth)
                .ThenBy(x => x.Contract.Person.FullName)
                .Select(x => new CrewPayrollResponse
                {
                    Id = x.Id,
                    PersonFullName = x.Contract.Person.FullName,
                    VesselName = x.Contract.Vessel.VesselName,
                    PayrollMonth = x.PayrollMonth,
                    RemainingBalance = x.RemainingBalance,
                    Status = x.Status
                })
                .ToListAsync(ct);
        }

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<CrewPayrollResponse> CreateAsync(int contractId, DateOnly payrollMonth, decimal allowances = 0m,
            decimal deductions = 0m, string? notes = null,
            CancellationToken ct = default)
        {
            await EnsureContractActiveAsync(contractId, ct);
            await EnsureNoDuplicatePayrollAsync(contractId, payrollMonth, excludeId: null, ct);
            var contract = await _contractRepo.GetByIdAsync(contractId, ct)
                ?? throw new KeyNotFoundException("contract not found");

            // 1. احسب عدد الأيام
            var workingDays = _CalculatorService.GetWorkingDays(contract, payrollMonth);

            // 2. احسب عدد أيام الشهر
            var totalDays = DateTime.DaysInMonth(payrollMonth.Year, payrollMonth.Month);

            // 3. احسب الراتب الأساسي
            var basicWage = _CalculatorService.CalculateBasicWage(
                contract.MonthlyWage,
                workingDays,
                totalDays);

            var payroll = CrewPayroll.Create(contractId, payrollMonth, workingDays,
                                             basicWage, allowances, deductions, notes);
            await _repo.AddAsync(payroll, ct);
            await _repo.SaveChangesAsync(ct);
            
            return new CrewPayrollResponse
            {
                WorkingDays = payroll.WorkingDays,
                Allowances = payroll.Allowances,
                GrossAmount = payroll.GrossAmount,
                BasicWage = payroll.BasicWage,
                ContractId = payroll.ContractId,
                Deductions = payroll.Deductions,
                Status = payroll.Status,
                IsFullyPaid = payroll.IsFullyPaid,
                RemainingBalance = payroll.RemainingBalance,
                TotalDisbursed = payroll.TotalDisbursed,
                Notes = payroll.Notes,
                PayrollMonth = payroll.PayrollMonth,
                Id = payroll.Id,
            };
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
                .AnyAsync(x => x.Id == contractId && x.IsActive, ct);
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
                .AnyAsync(x => x.Id == voyageId, ct);
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
