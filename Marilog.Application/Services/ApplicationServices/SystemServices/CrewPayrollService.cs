using Marilog.Contracts.DTOs.Reports.CrewPayrollReports;
using Marilog.Contracts.DTOs.Requests.CrewPayrollDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Domain.Entities.SystemEntities;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Kernel.Enums;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Marilog.Application.Services.ApplicationServices.SystemServices
{
    public class CrewPayrollService : ICrewPayrollService
    {
        private readonly IRepository<CrewPayroll> _repo;
        private readonly IRepository<CrewContract> _contractRepo;
        private readonly IRepository<Currency> _currencyRepo;

        public CrewPayrollService(
            IRepository<CrewPayroll> repo,
            IRepository<CrewContract> contractRepo,
            IRepository<Currency> currencyRepo)
        {
            _repo = repo;
            _contractRepo = contractRepo;
            _currencyRepo = currencyRepo;
        }

        // ── Queries ───────────────────────────────────────────────────────────────

        public Task<CrewPayrollResponse?> GetByIdAsync(int id, CancellationToken ct = default)
            => FindAsync(x => x.Id == id, ct);

        public Task<CrewPayrollResponse?> GetWithDisbursementsAsync(int id, CancellationToken ct = default)
            => FindAsync(x => x.Id == id, ct);

        public Task<CrewPayrollResponse?> GetByContractAndMonthAsync(int contractId, DateOnly month,
            CancellationToken ct = default)
        {
            var firstDay = FirstDayOf(month);
            return FindAsync(x => x.ContractId == contractId &&
                      x.PayrollMonth == firstDay &&
                      x.Status != PayrollStatus.Cancelled, ct);
        }

        public Task<IReadOnlyList<CrewPayrollResponse>> GetByContractAsync(int contractId,
            CancellationToken ct = default)
            => ListAsync(_repo.Query()
                    .Where(x => x.ContractId == contractId)
                    .OrderByDescending(x => x.PayrollMonth), ct);

        public Task<IReadOnlyList<CrewPayrollResponse>> GetByMonthAsync(DateOnly month,
            CancellationToken ct = default)
        {
            var firstDay = FirstDayOf(month);
            return ListAsync(_repo.Query()
                    .Where(x => x.PayrollMonth == firstDay)
                    .OrderBy(x => x.Contract.Person.FullName), ct);
        }

        public Task<IReadOnlyList<CrewPayrollResponse>> GetByStatusAsync(PayrollStatus status,
            CancellationToken ct = default)
            => ListAsync(_repo.Query()
                    .Where(x => x.Status == status)
                    .OrderByDescending(x => x.PayrollMonth), ct);

        public Task<IReadOnlyList<CrewPayrollResponse>> GetOutstandingAsync(CancellationToken ct = default)
            => ListAsync(_repo.Query()
                    .Where(x => x.Status == PayrollStatus.Approved ||
                                x.Status == PayrollStatus.PartiallyPaid)
                    .OrderBy(x => x.PayrollMonth)
                    .ThenBy(x => x.Contract.Person.FullName), ct);

        // ── Report ────────────────────────────────────────────────────────────────

        public async Task<CrewPayrollReport> GetCrewPayrollReportAsync(
            CrewPayrollFilterOptions options, CancellationToken ct = default)
        {
            var query = _repo.Query().AsNoTracking();

            if (options.ContractId.HasValue)
                query = query.Where(x => x.ContractId == options.ContractId.Value);

            if (options.PersonId.HasValue)
                query = query.Where(x => x.Contract.PersonID == options.PersonId.Value);

            if (options.VesselId.HasValue)
                query = query.Where(x => x.Contract.VesselID == options.VesselId.Value);

            if (options.Month.HasValue)
            {
                var firstDay = new DateOnly(options.Month.Value.Year, options.Month.Value.Month, 1);
                query = query.Where(x => x.PayrollMonth == firstDay);
            }
            if (options.CurrencyId <= 0)
                throw new ArgumentException("CurrencyId is required for the payroll report.");
            query = query.Where(x => x.CurrencyId == options.CurrencyId);

            // Cancelled payrolls never count toward totals unless explicitly requested
            if (options.OnlyOutstanding)
                query = query.Where(x => x.Status == PayrollStatus.Approved ||
                                         x.Status == PayrollStatus.PartiallyPaid);
            else if (options.Status.HasValue)
                query = query.Where(x => x.Status == options.Status.Value);
            else
                query = query.Where(x => x.Status != PayrollStatus.Cancelled);

            // single round-trip — all totals computed from the loaded rows
            var payrolls = await query
                .OrderBy(x => x.PayrollMonth)
                .ThenBy(x => x.Contract.Person.FullName)
                .Select(options.IncludeDisbursements ? DetailProjection : SummaryProjection)
                .ToListAsync(ct);

            var hasRows = payrolls.Count > 0;

            return new CrewPayrollReport
            {
                Payrolls = payrolls,
                TotalGross = payrolls.Sum(p => p.GrossAmount),
                TotalDisbursed = payrolls.Sum(p => p.TotalDisbursed),
                TotalRemaining = payrolls.Sum(p => p.RemainingBalance),
                AverageGross = hasRows ? Math.Round(payrolls.Average(p => p.GrossAmount), 2) : 0m,
                MaxGross = hasRows ? payrolls.Max(p => p.GrossAmount) : 0m,
                MinGross = hasRows ? payrolls.Min(p => p.GrossAmount) : 0m,

                MonthlySummary = payrolls
                    .GroupBy(p => new { p.PayrollMonth.Year, p.PayrollMonth.Month })
                    .Select(g => new MonthlyPayrollSummary
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        TotalGross = g.Sum(p => p.GrossAmount),
                        TotalDisbursed = g.Sum(p => p.TotalDisbursed),
                        TotalRemaining = g.Sum(p => p.RemainingBalance),
                    })
                    .OrderBy(m => m.Year).ThenBy(m => m.Month)
                    .ToList(),

                VesselSummary = payrolls
                    .GroupBy(p => p.VesselId)
                    .Select(g => new VesselPayrollSummary
                    {
                        VesselId = g.Key,
                        VesselName = g.First().VesselName,
                        TotalGross = g.Sum(p => p.GrossAmount),
                        TotalDisbursed = g.Sum(p => p.TotalDisbursed),
                        TotalRemaining = g.Sum(p => p.RemainingBalance),
                        ContractsCount = g.Select(p => p.ContractId).Distinct().Count()
                    })
                    .OrderBy(v => v.VesselName)
                    .ToList(),

                PersonSummary = payrolls
                    .GroupBy(p => p.PersonId)
                    .Select(g => new PersonPayrollSummary
                    {
                        PersonId = g.Key,
                        PersonFullName = g.First().PersonFullName,
                        TotalGross = g.Sum(p => p.GrossAmount),
                        TotalDisbursed = g.Sum(p => p.TotalDisbursed),
                        TotalRemaining = g.Sum(p => p.RemainingBalance),
                        ContractsCount = g.Select(p => p.ContractId).Distinct().Count()
                    })
                    .OrderBy(p => p.PersonFullName)
                    .ToList(),
            };
        }

        // ── Commands ──────────────────────────────────────────────────────────────

        public async Task<CrewPayrollResponse> CreateAsync(int contractId, DateOnly payrollMonth, int currencyId,
            decimal allowances = 0m, decimal deductions = 0m,  string? notes = null, CancellationToken ct = default)
        {
            var contract = await GetContractOrThrowAsync(contractId, ct);
            await EnsureCurrencyExistsAsync(currencyId, ct);
            await EnsureNoDuplicatePayrollAsync(contractId, payrollMonth, ct);

            var payroll = CrewPayroll.Create(contract, payrollMonth, currencyId, allowances, deductions, notes);

            await _repo.AddAsync(payroll, ct);
            await _repo.SaveChangesAsync(ct);

            return (await GetByIdAsync(payroll.Id, ct))!;
        }

        public async Task<IReadOnlyList<CrewPayrollResponse>> CreateRangeAsync(
            IEnumerable<CreateCrewPayrollRequest> commands, CancellationToken ct = default)
        {
            var items = commands
                .Select(c => new
                {
                    c.ContractId,
                    Month = FirstDayOf(c.PayrollMonth),
                    c.CurrencyId,
                    c.Allowances,
                    c.Deductions,
                    c.Notes
                })
                .ToList();

            if (items.Count == 0) return Array.Empty<CrewPayrollResponse>();

            // duplicates inside the batch itself
            var duplicate = items.GroupBy(i => (i.ContractId, i.Month)).FirstOrDefault(g => g.Count() > 1);
            if (duplicate is not null)
                throw new InvalidOperationException(
                    $"Contract {duplicate.Key.ContractId} appears more than once for {duplicate.Key.Month:yyyy-MM} in this batch.");

            var contractIds = items.Select(i => i.ContractId).Distinct().ToList();
            var months = items.Select(i => i.Month).Distinct().ToList();

            var contracts = await _contractRepo.Query()
                    .AsNoTracking()
                    .Where(x => contractIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, ct);


            var currencyIds = items.Select(i => i.CurrencyId).Distinct().ToList();
            var validCurrencyIds = await _currencyRepo.Query()
                .AsNoTracking()
                .Where(x => currencyIds.Contains(x.Id) && x.IsActive)
                .Select(x => x.Id)
                .ToListAsync(ct);

            var missingCurrency = currencyIds.Except(validCurrencyIds).ToList();
            if (missingCurrency.Count > 0)
                throw new KeyNotFoundException($"Currency {missingCurrency[0]} not found or inactive.");
            var existing = (await _repo.Query()
                    .AsNoTracking()
                    .Where(x => contractIds.Contains(x.ContractId) && months.Contains(x.PayrollMonth) && x.Status != PayrollStatus.Cancelled)
                    .Select(x => new { x.ContractId, x.PayrollMonth })
                    .ToListAsync(ct))
                .Select(x => (x.ContractId, x.PayrollMonth))
                .ToHashSet();

            var payrolls = new List<CrewPayroll>(items.Count);
            foreach (var i in items)
            {
                if (!contracts.TryGetValue(i.ContractId, out var contract))
                    throw new KeyNotFoundException($"CrewContract {i.ContractId} not found.");

                if (existing.Contains((i.ContractId, i.Month)))
                    throw new InvalidOperationException(
                        $"A payroll already exists for contract {i.ContractId} in {i.Month:yyyy-MM}.");

                payrolls.Add(CrewPayroll.Create(contract, i.Month, i.CurrencyId, i.Allowances, i.Deductions, i.Notes));
            }

            await _repo.AddRangeAsync(payrolls, ct);
            await _repo.SaveChangesAsync(ct);

            var ids = payrolls.Select(p => p.Id).ToList();
            return await ListAsync(_repo.Query()
                    .Where(x => ids.Contains(x.Id))
                    .OrderBy(x => x.Contract.Person.FullName), ct);
        }

        public async Task UpdateAsync(int id, int contractId, int currencyId,
                                      decimal allowances = 0m, decimal deductions = 0m, string? notes = null,
                                      CancellationToken ct = default)
        {
            var payroll = await GetOrThrowAsync(id, ct);

            if (contractId != payroll.ContractId)
                throw new InvalidOperationException(
                    "A payroll's contract cannot be changed. Delete it and create a new one.");

            await EnsureCurrencyExistsAsync(currencyId, ct);
            var contract = await GetContractOrThrowAsync(payroll.ContractId, ct);
            payroll.Update(contract, currencyId, allowances, deductions, notes);

            await SaveAsync(payroll, ct);
        }

        public async Task ApproveAsync(int id, CancellationToken ct = default)
        {
            var payroll = await GetOrThrowAsync(id, ct);
            var contract = await GetContractOrThrowAsync(payroll.ContractId, ct);
            payroll.Approve(contract);
            await SaveAsync(payroll, ct);
        }

        public async Task CancelAsync(int id, string reason, CancellationToken ct = default)
        {
            var payroll = await GetOrThrowAsync(id, ct);   // loads disbursements — required by the guard
            payroll.Cancel(reason);
            await SaveAsync(payroll, ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var payroll = await GetOrThrowAsync(id, ct);

            if (payroll.Status != PayrollStatus.Draft)
                throw new InvalidOperationException("Only Draft payrolls can be deleted.");

            _repo.HardDelete(payroll);
            await _repo.SaveChangesAsync(ct);
        }

        // ── Disbursements ─────────────────────────────────────────────────────────

        public async Task<DisbursementResponse> PayAsync(int payrollId, PayCrewPayrollRequest request, CancellationToken ct = default)
        {
            var payroll = await GetOrThrowAsync(payrollId, ct);

            var disbursement = payroll.Pay(request.Method, request.Amount, request.PaidOn,
                                           request.RecipientName, request.RecipientIdNumber,
                                           request.Reference, request.Notes);

            return await SaveAndReadDisbursementAsync(payroll, disbursement, ct);
        }

        public async Task ConfirmDisbursementAsync(int payrollId, int disbursementId,
            CancellationToken ct = default)
        {
            var payroll = await GetOrThrowAsync(payrollId, ct);
            payroll.ConfirmDisbursement(disbursementId);
            await SaveAsync(payroll, ct);
        }

        public async Task CancelDisbursementAsync(int payrollId, int disbursementId,
            string reason, CancellationToken ct = default)
        {
            var payroll = await GetOrThrowAsync(payrollId, ct);
            payroll.CancelDisbursement(disbursementId, reason);
            await SaveAsync(payroll, ct);
        }

        // ── Private: read helpers ─────────────────────────────────────────────────

        private Task<CrewPayrollResponse?> FindAsync(
            Expression<Func<CrewPayroll, bool>> predicate, CancellationToken ct)
            => _repo.Query()
                    .AsNoTracking()
                    .Where(predicate)
                    .Select(DetailProjection)
                    .FirstOrDefaultAsync(ct);

        private static async Task<IReadOnlyList<CrewPayrollResponse>> ListAsync(
            IQueryable<CrewPayroll> query, CancellationToken ct)
            => await query.AsNoTracking()
                          .Select(SummaryProjection)
                          .ToListAsync(ct);

        // ── Private: write helpers ────────────────────────────────────────────────

        /// <summary>Loads the aggregate with its disbursements — every command needs them.</summary>
        private async Task<CrewPayroll> GetOrThrowAsync(int id, CancellationToken ct)
            => await _repo.Query()
                          .Include(x => x.Disbursements)
                          .FirstOrDefaultAsync(x => x.Id == id, ct)
               ?? throw new KeyNotFoundException($"CrewPayroll {id} not found.");

        private async Task SaveAsync(CrewPayroll payroll, CancellationToken ct)
        {
            _repo.Update(payroll);
            await _repo.SaveChangesAsync(ct);
        }

        private async Task<DisbursementResponse> SaveAndReadDisbursementAsync(
            CrewPayroll payroll, CrewPayrollDisbursement disbursement, CancellationToken ct)
        {
            await SaveAsync(payroll, ct);

            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.Id == payroll.Id)
                .SelectMany(x => x.Disbursements)
                .Where(d => d.Id == disbursement.Id)
                .Select(DisbursementProjection)
                .FirstAsync(ct);
        }

        // ── Private: guards ───────────────────────────────────────────────────────



        private async Task EnsureCurrencyExistsAsync(int currencyId, CancellationToken ct)
        {
            if (!await _currencyRepo.Query().AnyAsync(x => x.Id == currencyId && x.IsActive, ct))
                throw new KeyNotFoundException($"Currency {currencyId} not found or inactive.");
        }

        private async Task<CrewContract> GetContractOrThrowAsync(int contractId, CancellationToken ct)
        => await _contractRepo.Query()
                          .AsNoTracking()
                          .FirstOrDefaultAsync(x => x.Id == contractId, ct)
       ?? throw new KeyNotFoundException($"CrewContract {contractId} not found.");

        private async Task EnsureNoDuplicatePayrollAsync(int contractId, DateOnly month, CancellationToken ct)
        {
            var firstDay = FirstDayOf(month);
            if (await _repo.Query().AnyAsync(x => x.ContractId == contractId &&
                                      x.PayrollMonth == firstDay &&
                                      x.Status != PayrollStatus.Cancelled, ct))
                throw new InvalidOperationException(
                    $"A payroll already exists for contract {contractId} in {firstDay:yyyy-MM}.");
        }

        private static DateOnly FirstDayOf(DateOnly date) => new(date.Year, date.Month, 1);

        // ── Projections ───────────────────────────────────────────────────────────

        private static readonly Expression<Func<CrewPayrollDisbursement, DisbursementResponse>> DisbursementProjection =
        d => new DisbursementResponse
        {
            Id = d.Id,
            Method = d.Method,
            Amount = d.Amount,
            PaidOn = d.PaidOn,
            Status = d.Status,
            RecipientName = d.RecipientName,
            RecipientIdNumber = d.RecipientIdNumber,
            Reference = d.Reference,
            Notes = d.Notes,
            CancelReason = d.CancelReason
     };

        private static readonly Expression<Func<CrewPayroll, CrewPayrollResponse>> SummaryProjection =
            BuildProjection(includeDisbursements: false);

        private static readonly Expression<Func<CrewPayroll, CrewPayrollResponse>> DetailProjection =
            BuildProjection(includeDisbursements: true);

        private static Expression<Func<CrewPayroll, CrewPayrollResponse>> BuildProjection(bool includeDisbursements) =>
            x => new CrewPayrollResponse
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
                CurrencyId = x.CurrencyId,
                CurrencyCode = x.Currency.CurrencyCode,
                TotalDisbursed = x.Disbursements
                                    .Where(d => d.Status != DisbursementStatus.Cancelled)
                                    .Sum(d => d.Amount),
                RemainingBalance = x.GrossAmount - x.Disbursements
                                    .Where(d => d.Status != DisbursementStatus.Cancelled)
                                    .Sum(d => d.Amount),
                IsFullyPaid = x.Status == PayrollStatus.FullyPaid,
                Status = x.Status,
                Notes = x.Notes,
                CancelReason = x.CancelReason,
                Disbursements = includeDisbursements
                                    ? x.Disbursements.AsQueryable().Select(DisbursementProjection).ToList()
                                    : null
            };
    }
}