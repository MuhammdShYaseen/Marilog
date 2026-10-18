using Marilog.Contracts.DTOs.Reports.CrewContractReports;
using Marilog.Contracts.DTOs.Requests.CrewDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Domain.Entities.SystemEntities;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Kernel.Enums;
using Marilog.Kernel.Primitives;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Marilog.Application.Services.ApplicationServices.SystemServices
{
    public class CrewContractService : ICrewContractService
    {
        private readonly IRepository<CrewContract> _repo;
        private readonly IRepository<Vessel> _vesselRepo;
        private readonly IRepository<Person> _personRepo;
        private readonly IRepository<Rank> _rankRepo;
        private readonly IRepository<CrewPayroll> _payrollRepo;

        public CrewContractService(
            IRepository<CrewContract> repo,
            IRepository<Vessel> vesselRepo,
            IRepository<Person> personRepo,
            IRepository<Rank> rankRepo,
            IRepository<CrewPayroll> payrollRepo)
        {
            _repo = repo;
            _vesselRepo = vesselRepo;
            _personRepo = personRepo;
            _rankRepo = rankRepo;
            _payrollRepo = payrollRepo;
        }

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<CrewContractResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var response = await _repo
                .Query()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(ToResponse())
                .FirstOrDefaultAsync(ct);
            //if (response != null)
                //SetCrewContractStatus(response);
            return response;
        }

        public async Task<IReadOnlyList<CrewContractResponse>> GetByPersonAsync(int personId,
            CancellationToken ct = default)
        {
            var response = await _repo
                .Query()
                .AsNoTracking()
                .Where(x => x.PersonID == personId)
                .OrderByDescending(x => x.SignOnDate)
                .Select(ToResponse())
                .ToListAsync(ct);
            //SetCrewContractStatus(response);
            return response;
        }

        public async Task<IReadOnlyList<CrewContractResponse>> GetByVesselAsync(int vesselId,
            CancellationToken ct = default)
        {
            var response = await _repo
                .Query()
                .AsNoTracking()
                .Where(x => x.VesselID == vesselId)
                .OrderByDescending(x => x.SignOnDate)
                .Select(ToResponse())
                .ToListAsync(ct);
            //SetCrewContractStatus(response);
            return response;
        }
        public async Task<IReadOnlyList<CrewContractResponse>> GetExpiredAsync(CancellationToken ct)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var response = await _repo
                .Query()
                .AsNoTracking()
                .Where(c => c.SignOnDate.AddMonths(c.DurationInMonth!.Value) < today && c.IsActive == true)
                .Select(ToResponse())
                .ToListAsync(ct);
            //SetCrewContractStatus(response);
            return response;
        }
        private void SetCrewContractStatus(CrewContractResponse response)
        {
            if (!response.IsActive || !response.ContractDurationMonths.HasValue)
            {
                response.ContractStatus = CrewContractStatus.Expired;
                return;
            }

            var today = DateOnly.FromDateTime(DateTime.Today);
            var expiryDate = response.SignOnDate.AddMonths(response.ContractDurationMonths.Value);

            if (expiryDate < today)
            {
                response.ContractStatus = CrewContractStatus.Expired;
            }
            else if (expiryDate <= today.AddMonths(2))
            {
                response.ContractStatus = CrewContractStatus.Expiring;
            }
            else
            {
                response.ContractStatus = CrewContractStatus.Valied;
            }
        }

        private void SetCrewContractStatus(List<CrewContractResponse> responses)
        {
            foreach (var res in responses)
                SetCrewContractStatus(res);
        }
        public async Task<IReadOnlyList<CrewContractResponse>> GetAboutExpireAsync(CancellationToken ct)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var nextTwoMonths = today.AddMonths(2);

            var response = await _repo.Query()
                .AsNoTracking()
                .Where(c => c.SignOnDate.AddMonths(c.DurationInMonth!.Value) >= today && c.SignOnDate.AddMonths(c.DurationInMonth!.Value) <= nextTwoMonths && c.IsActive == true)
                .Select(ToResponse())
                .ToListAsync(ct);
            //SetCrewContractStatus(response);
            return response;
        }
        public async Task<IReadOnlyList<CrewContractResponse>> GetActiveByVesselAsync(int vesselId,
            CancellationToken ct = default)
        {
            var response = await _repo.Query()
                .AsNoTracking()
                .Where(x => x.VesselID == vesselId && x.IsActive)
                .OrderBy(x => x.Rank.Department)
                .ThenBy(x => x.Person.FullName)
                .Select(ToResponse())
                .ToListAsync(ct);
            //SetCrewContractStatus(response);
            return response;
        }

        public async Task<CrewContractResponse?> GetActiveMasterAsync(int vesselId,
            CancellationToken ct = default)
        {
            var response = await _repo.Query()
                .AsNoTracking()
                .Where(x => x.VesselID == vesselId && x.IsActive && x.Rank.RankCode == "MASTER")
                .Select(ToResponse())
                .FirstOrDefaultAsync(ct);
            //if(response != null)
                //SetCrewContractStatus(response);
            return response;
        }

        public async Task<IReadOnlyList<CrewContractResponse>> GetActiveOnDateAsync(DateOnly date,
            CancellationToken ct = default)
        {
            var response = await _repo.Query()
                .AsNoTracking()
                .Where(x => x.SignOnDate <= date && (!x.SignOffDate.HasValue || x.SignOffDate.Value >= date))
                .Select(ToResponse())
                .ToListAsync(ct);
                //SetCrewContractStatus(response);
            return response;
        }

        public async Task<IReadOnlyList<CrewContractResponse>> GetAllAsync(CancellationToken ct = default)
        {
            var response = await _repo
                              .Query()
                              .AsNoTracking()
                              .Select(ToResponse())
                              .ToListAsync(ct);
            //SetCrewContractStatus(response);
            return response;
        }

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<CrewContractResponse> CreateAsync(int durationInMonth, int personId, int vesselId, int rankId,
            decimal monthlyWage, DateOnly signOnDate, int? signOnPort = null,
            string? notes = null, CancellationToken ct = default)
        {
            await EnsurePersonExistsAsync(personId, ct);
            await EnsureVesselActiveAsync(vesselId, ct);
            await EnsureRankExistsAsync(rankId, ct);
            await EnsureNoOverlappingContractAsync(personId, signOnDate, signOffDate: null, excludeId: null, ct);

            var contract = CrewContract.Create(durationInMonth, personId, vesselId, rankId,
                                               monthlyWage, signOnDate, signOnPort, notes);
            await _repo.AddAsync(contract, ct);
            await _repo.SaveChangesAsync(ct);

            return await _repo.Query()
                              .AsNoTracking()
                              .Where(x => x.Id == contract.Id)
                              .Select(ToResponse())
                              .FirstAsync(ct);
        }

        public async Task<IReadOnlyList<CrewContractResponse>> CreateRangeAsync(
    IEnumerable<CreateCrewContractRequest> commands,
    CancellationToken ct = default)
        {
            var items = commands.ToList();
            if (items.Count == 0) return Array.Empty<CrewContractResponse>();

            // ── 1. Same person twice in one batch — two open contracts always overlap
            var duplicatePerson = items.GroupBy(i => i.PersonId).FirstOrDefault(g => g.Count() > 1);
            if (duplicatePerson is not null)
                throw new InvalidOperationException(
                    $"Person {duplicatePerson.Key} appears more than once in this batch.");

            // ── 2. Persons, vessels and ranks exist and are active (one query each)
            var personIds = items.Select(i => i.PersonId).Distinct().ToList();
            var vesselIds = items.Select(i => i.VesselId).Distinct().ToList();
            var rankIds = items.Select(i => i.RankId).Distinct().ToList();

            var validPersonIds = await _personRepo.Query().AsNoTracking()
                .Where(x => personIds.Contains(x.Id) && x.IsActive)
                .Select(x => x.Id)
                .ToListAsync(ct);
            EnsureAllFound(personIds, validPersonIds, "Person");

            var validVesselIds = await _vesselRepo.Query().AsNoTracking()
                .Where(x => vesselIds.Contains(x.Id) && x.IsActive)
                .Select(x => x.Id)
                .ToListAsync(ct);
            EnsureAllFound(vesselIds, validVesselIds, "Vessel");

            var validRankIds = await _rankRepo.Query().AsNoTracking()
                .Where(x => rankIds.Contains(x.Id) && x.IsActive)
                .Select(x => x.Id)
                .ToListAsync(ct);
            EnsureAllFound(rankIds, validRankIds, "Rank");

            // ── 3. No overlap with existing contracts on any vessel (one query)
            //       New contracts are open-ended, so any contract still running on or after
            //       the new sign-on date — including a future one — is a conflict.
            var earliestSignOn = items.Min(i => i.SignOnDate);

            var existing = await _repo.Query().AsNoTracking()
                .Where(x => personIds.Contains(x.PersonID) &&
                            (!x.SignOffDate.HasValue || x.SignOffDate.Value >= earliestSignOn))
                .Select(x => new { x.Id, x.PersonID, x.Vessel.VesselName, x.SignOnDate, x.SignOffDate })
                .ToListAsync(ct);

            foreach (var item in items)
            {
                var conflict = existing.FirstOrDefault(x =>
                    x.PersonID == item.PersonId &&
                    (!x.SignOffDate.HasValue || x.SignOffDate.Value >= item.SignOnDate));

                if (conflict is not null)
                    throw new InvalidOperationException(
                        $"Person {item.PersonId}: overlaps contract {conflict.Id} on {conflict.VesselName} " +
                        $"({conflict.SignOnDate:dd MMM yyyy} – {conflict.SignOffDate?.ToString("dd MMM yyyy") ?? "on board"}). " +
                        "For a same-day transfer, sign on the new vessel the day after sign-off.");
            }

            // ── 4. Create — the domain validates duration and wage
            var contracts = items
                .Select(c => CrewContract.Create(
                    c.DurationInMonth,
                    c.PersonId,
                    c.VesselId,
                    c.RankId,
                    c.MonthlyWage,
                    c.SignOnDate,
                    c.SignOnPort,
                    c.Notes))
                .ToList();

            await _repo.AddRangeAsync(contracts, ct);
            await _repo.SaveChangesAsync(ct);

            var ids = contracts.Select(c => c.Id).ToList();
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => ids.Contains(x.Id))
                .OrderBy(x => x.Vessel.VesselName)
                .ThenBy(x => x.Person.FullName)
                .Select(ToResponse())
                .ToListAsync(ct);
        }

        public async Task<Result> UpdateAsync(int id, int durationInMonth, int personId, int vesselId, int rankId,
                                      decimal monthlyWage, DateOnly signOnDate,
                                      int? signOnPort = null, string? notes = null,
                                      CancellationToken ct = default)
        {
            var contract = await GetOrThrowAsync(id, ct);

            await EnsureNoOverlappingContractAsync(personId, signOnDate, contract.SignOffDate, excludeId: id, ct);

            if (personId != contract.PersonID || vesselId != contract.VesselID || signOnDate != contract.SignOnDate)
                await EnsureNoPostedPayrollsAsync(id,
                    "Person, vessel and sign-on date cannot change after a payroll has been approved. " +
                    "Cancel the approved payrolls first.", ct);

            contract.Update(durationInMonth, personId, vesselId, rankId,
                            monthlyWage, signOnDate, signOnPort, notes);
            _repo.Update(contract);
            await _repo.SaveChangesAsync(ct);
            return Result.Ok();
        }

        public async Task<Result> SignOffAsync(int id, DateOnly signOffDate, int? signOffPort = null, CancellationToken ct = default)
        {
            var contract = await GetOrThrowAsync(id, ct);

            // first payroll month whose approved days would be cut by this sign-off
            var offMonth = new DateOnly(signOffDate.Year, signOffDate.Month, 1);
            var isLastDayOfMonth = signOffDate.Day == DateTime.DaysInMonth(signOffDate.Year, signOffDate.Month);
            var firstAffectedMonth = isLastDayOfMonth ? offMonth.AddMonths(1) : offMonth;

            var affected = await _payrollRepo.Query()
                .AsNoTracking()
                .Where(x => x.ContractId == id &&
                            x.Status != PayrollStatus.Draft &&
                            x.Status != PayrollStatus.Cancelled &&
                            x.PayrollMonth >= firstAffectedMonth)
                .OrderBy(x => x.PayrollMonth)
                .Select(x => (DateOnly?)x.PayrollMonth)
                .FirstOrDefaultAsync(ct);

            if (affected.HasValue)
                throw new InvalidOperationException(
                    $"The {affected.Value:MMM yyyy} payroll is already approved with service past {signOffDate:dd MMM yyyy}. " +
                    "Cancel it (voiding its payments first) before signing off.");

            contract.SignOff(signOffDate, signOffPort);
            _repo.Update(contract);
            await _repo.SaveChangesAsync(ct);
            return Result.Ok();
        }

        public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
        {
            var contract = await GetOrThrowAsync(id, ct);
            if (!contract.SignOffDate.HasValue)
                throw new InvalidOperationException(
                    "Cannot delete an active contract. Sign off the crew member first.");
            if (await _payrollRepo.Query().AnyAsync(x => x.ContractId == id, ct))
                throw new InvalidOperationException("Cannot delete a contract that has payrolls.");
            _repo.HardDelete(contract);
            await _repo.SaveChangesAsync(ct);
            return Result.Ok();
        }

        //----Reports----------------------------------------------------------------

        public async Task<CrewContractReport> GetCrewContractsReportAsync(CrewContractFilterOptions options, CancellationToken ct = default)
        {
            var query = _repo.Query().AsNoTracking();

            // ─── فلترة ───────────────────────────────────────────────────────────
            if (options.VesselId.HasValue)
                query = query.Where(x => x.VesselID == options.VesselId.Value);

            if (options.PersonId.HasValue)
                query = query.Where(x => x.PersonID == options.PersonId.Value);

            if (options.IsActive.HasValue)
                query = query.Where(x => x.IsActive == options.IsActive.Value);

            // ✅ OnlyMasters يأخذ الأولوية على RankCode
            if (options.OnlyMasters)
                query = query.Where(x => x.Rank.RankCode == "MASTER");
            else if (!string.IsNullOrWhiteSpace(options.RankCode))
                query = query.Where(x => x.Rank.RankCode == options.RankCode);

            if (options.OnDate.HasValue)
            {
                var date = options.OnDate.Value;
                query = query.Where(x => x.SignOnDate <= date &&
                                         (!x.SignOffDate.HasValue || x.SignOffDate.Value >= date));
            }

            // ─── ترتيب ───────────────────────────────────────────────────────────
            query = query.OrderBy(x => x.Rank.Department)
                         .ThenBy(x => x.Person.FullName);

            // ─── الإحصاءات من DB — رحلة واحدة ───────────────────────────────────
            var dbSummary = await query
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    TotalCount = g.Count(),
                    ActiveCount = g.Count(x => x.IsActive),
                    InactiveCount = g.Count(x => !x.IsActive),
                })
                .FirstOrDefaultAsync(ct);

            // ─── جلب البيانات ────────────────────────────────────────────────────
            var contracts = await query.Select(x => new CrewContractResponse
            {
                ContractId = x.Id,
                PersonId = x.PersonID,
                PersonFullName = x.Person.FullName,
                VesselId = x.VesselID,
                VesselName = x.Vessel.VesselName,
                RankId = x.RankID,
                RankName = x.Rank.RankName,
                RankDepartment = x.Rank.Department,
                MonthlyWage = x.MonthlyWage,
                SignOnDate = x.SignOnDate,
                SignOffDate = x.SignOffDate,
                SignOnPort = x.SignOnPort,
                SignOnPortName = x.SignOnPortNav != null ? x.SignOnPortNav.PortName : null,   // ✅ آمن
                SignOffPort = x.SignOffPort,
                SignOffPortName = x.SignOffPortNav != null ? x.SignOffPortNav.PortName : null, // ✅ آمن
                ContractDurationMonths = x.DurationInMonth != null ? x.DurationInMonth : null,
                IsActive = x.IsActive
            }).ToListAsync(ct);

            // ─── التجميع حسب السفينة ───────────────────────────────
            var vesselSummary = contracts
                .GroupBy(c => c.VesselId)
                .Select(g => new VesselContractSummary
                {
                    VesselId = g.Key,
                    VesselName = g.Select(x => x.VesselName).FirstOrDefault() ?? "",
                    TotalContracts = g.Count(),
                    ActiveContracts = g.Count(c => c.IsActive),
                    AverageWage = g.Any() ? g.Average(c => c.MonthlyWage) : 0m
                })
                .OrderBy(v => v.VesselName)
                .ToList();

            // ─── التجميع حسب الرتبة ───────────────────────────────
            var rankSummary = contracts
                .GroupBy(c => c.RankId)
                .Select(g => new RankContractSummary
                {
                    RankId = g.Key,
                    RankName = g.Select(x => x.RankName).FirstOrDefault() ?? "",
                    Department = g.Select(x => x.RankDepartment).FirstOrDefault(),
                    TotalContracts = g.Count(),
                    ActiveContracts = g.Count(c => c.IsActive),
                    AverageWage = g.Average(c => c.MonthlyWage)   // ✅
                })
                .OrderBy(r => r.Department)
                .ThenBy(r => r.RankName)
                .ToList();

            // ─── التجميع حسب القسم ───────────────────────────────────────────────
            var departmentSummary = contracts
                .GroupBy(c => c.RankDepartment)
                .Select(g => new DepartmentContractSummary
                {
                    Department = g.Key,
                    TotalContracts = g.Count(),
                    ActiveContracts = g.Count(c => c.IsActive),
                })
                .OrderBy(d => d.Department)
                .ToList();

            // ─── النتيجة النهائية ─────────────────────────────────────────────────
            return new CrewContractReport
            {
                Contracts = contracts,
                TotalCount = dbSummary?.TotalCount ?? 0,
                ActiveCount = dbSummary?.ActiveCount ?? 0,
                InactiveCount = dbSummary?.InactiveCount ?? 0,
                VesselSummary = vesselSummary,
                RankSummary = rankSummary,
                DepartmentSummary = departmentSummary,
            };
        }

        // ── Private ───────────────────────────────────────────────────────────────
        private static void EnsureAllFound(List<int> requested, List<int> found, string entityName)
        {
            var missing = requested.Except(found).ToList();
            if (missing.Count > 0)
                throw new KeyNotFoundException(
                    $"{entityName} {string.Join(", ", missing)} not found or inactive.");
        }
        private async Task<CrewContract> GetOrThrowAsync(int id, CancellationToken ct)
            => await _repo.GetByIdAsync(id, ct)
               ?? throw new KeyNotFoundException($"CrewContract {id} not found.");

        private async Task EnsurePersonExistsAsync(int personId, CancellationToken ct)
        {
            var exists = await _personRepo.Query()
                .AnyAsync(x => x.Id == personId && x.IsActive, ct);
            if (!exists)
                throw new KeyNotFoundException($"Person {personId} not found or inactive.");
        }

        private async Task EnsureVesselActiveAsync(int vesselId, CancellationToken ct)
        {
            var exists = await _vesselRepo.Query()
                .AnyAsync(x => x.Id == vesselId && x.IsActive, ct);
            if (!exists)
                throw new KeyNotFoundException($"Vessel {vesselId} not found or inactive.");
        }

        private async Task EnsureRankExistsAsync(int rankId, CancellationToken ct)
        {
            var exists = await _rankRepo.Query()
                .AnyAsync(x => x.Id == rankId && x.IsActive, ct);
            if (!exists)
                throw new KeyNotFoundException($"Rank {rankId} not found or inactive.");
        }

        private async Task EnsureNoActiveContractAsync(int personId, int vesselId,
            DateOnly signOnDate, CancellationToken ct)
        {
            var conflict = await _repo.Query()
                .AnyAsync(x => x.PersonID   == personId &&
                               x.VesselID   == vesselId &&
                               x.IsActive               &&
                               x.SignOnDate <= signOnDate &&
                               (!x.SignOffDate.HasValue || x.SignOffDate.Value >= signOnDate), ct);
            if (conflict)
                throw new InvalidOperationException(
                    "This crew member already has an active contract on this vessel.");
        }

        private async Task EnsureNoOverlappingContractAsync(int personId, DateOnly signOnDate,
    DateOnly? signOffDate, int? excludeId, CancellationToken ct)
        {
            var end = signOffDate ?? DateOnly.MaxValue;

            var conflict = await _repo.Query()
                .AsNoTracking()
                .Where(x => x.PersonID == personId &&
                            (excludeId == null || x.Id != excludeId) &&
                            x.SignOnDate <= end &&
                            (!x.SignOffDate.HasValue || x.SignOffDate.Value >= signOnDate))
                .Select(x => new { x.Id, x.Vessel.VesselName, x.SignOnDate, x.SignOffDate })
                .FirstOrDefaultAsync(ct);

            if (conflict is not null)
                throw new InvalidOperationException(
                    $"Overlaps contract {conflict.Id} on {conflict.VesselName} " +
                    $"({conflict.SignOnDate:dd MMM yyyy} – {conflict.SignOffDate?.ToString("dd MMM yyyy") ?? "on board"}). " +
                    "For a same-day transfer, sign on the new vessel the day after sign-off.");
        }

        private async Task EnsureNoPostedPayrollsAsync(int contractId, string message, CancellationToken ct)
        {
            if (await _payrollRepo.Query().AnyAsync(x => x.ContractId == contractId &&
                                                         x.Status != PayrollStatus.Draft &&
                                                         x.Status != PayrollStatus.Cancelled, ct))
                throw new InvalidOperationException(message);
        }

        //---------Mapping---------------
        public static Expression<Func<CrewContract, CrewContractResponse>> ToResponse()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            return x => new CrewContractResponse
            {
                ContractId = x.Id,
                PersonId = x.PersonID,
                PersonFullName = x.Person.FullName,
                VesselId = x.VesselID,
                VesselName = x.Vessel.VesselName,
                RankId = x.RankID,
                RankName = x.Rank.RankName,
                RankDepartment = x.Rank.Department,
                MonthlyWage = x.MonthlyWage,
                SignOnDate = x.SignOnDate,
                SignOffDate = x.SignOffDate,
                SignOnPort = x.SignOnPort,
                SignOnPortName = x.SignOnPortNav != null ? x.SignOnPortNav.PortName : null,
                SignOffPort = x.SignOffPort,
                SignOffPortName = x.SignOffPortNav != null ? x.SignOffPortNav.PortName : null,
                ContractDurationMonths = x.DurationInMonth != null ? x.DurationInMonth : null,
                IsActive = x.IsActive,
                Notes = x.Notes,
                ContractDurationDays = x.SignOffDate.HasValue
                    ? x.SignOffDate.Value.DayNumber - x.SignOnDate.DayNumber
                    : today.DayNumber - x.SignOnDate.DayNumber,

                TotalWageEarned = x.SignOffDate.HasValue
                    ? x.MonthlyWage / 30m * (x.SignOffDate.Value.DayNumber - x.SignOnDate.DayNumber)
                    : x.MonthlyWage / 30m * (today.DayNumber - x.SignOnDate.DayNumber),
                Rank = new RankResponse
                {
                    Code = x.Rank.RankCode,
                    Name = x.Rank.RankName,
                    Department = x.Rank.Department,
                    IsActive = x.Rank.IsActive
                },
                ContractStatus =
                    !x.IsActive || !x.DurationInMonth.HasValue
                        ? CrewContractStatus.Expired
                            : x.SignOnDate.AddMonths(x.DurationInMonth.Value) < today
                                ? CrewContractStatus.Expired
                                    : x.SignOnDate.AddMonths(x.DurationInMonth.Value) <= today.AddMonths(2)
                                        ? CrewContractStatus.Expiring
                                            : CrewContractStatus.Valied,
            };
        }
    }
}

