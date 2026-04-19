using Marilog.Application.Common;
using Marilog.Application.Extensions;
using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Reports.Contract;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services;
using Marilog.Domain.Entities.SystemEntities;
using Marilog.Domain.Enumerations;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Domain.ValueObjects.Contract;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Marilog.Application.Services.ApplicationServices
{
    public class ContractService : IContractService
    {
        private readonly IRepository<AContract> _repository;
        //private readonly IDateTimeProvider _dateTime;
        public ContractService(IRepository<AContract> repository)
        {
            _repository = repository;
        }
        // ══════════════════════════════════════════════════════════════════
        // Queries
        // ══════════════════════════════════════════════════════════════════

        public async Task<ContractDetailResponse?> GetByIdAsync(int id, CancellationToken ct = default)
            => await _repository
                .Query()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(ToDetailResponse)
                .FirstOrDefaultAsync(ct);

        public async Task<ContractDetailResponse?> GetByNumberAsync(string number, CancellationToken ct = default)
            => await _repository
                .Query()
                .AsNoTracking()
                .Where(x => x.ContractNumber == number)
                .Select(ToDetailResponse)
                .FirstOrDefaultAsync(ct);

        public async Task<Contracts.Common.PagedResponse<ContractSummary>> GetPagedAsync(
            ContractFilter filter, CancellationToken ct = default)
        {
            var page = Math.Max(filter.Page, 1);
            var pageSize = Math.Min(Math.Max(filter.PageSize, 1), 100);

            var query = _repository
                .Query()
                .AsNoTracking()
                .ApplyFilter(filter);

            var total = await query.CountAsync(ct);

            var items = await query
                .ApplySort(filter) // MUST ensure default ordering
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(ToSummaryResponse) // MUST be Expression
                .ToListAsync(ct);

            return new Contracts.Common.PagedResponse<ContractSummary>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<List<ContractSummary>> GetExpiringAsync(
            int withinDays, CancellationToken ct = default)
        {
            var threshold = DateOnly.FromDateTime(DateTime.Today.AddDays(withinDays));

            return await _repository
                .Query()
                .AsNoTracking()
                .Where(x => x.ExpiryDate.HasValue
                         && x.ExpiryDate.Value >= DateOnly.FromDateTime(DateTime.Today)
                         && x.ExpiryDate.Value <= threshold
                         && x.Status.Id == ContractStatus.Active.Id)
                .OrderBy(x => x.ExpiryDate)
                .Select(ToSummaryResponse)
                .ToListAsync(ct);
        }

        public async Task<ContractReport> GetReportAsync(CancellationToken ct = default)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var threshold = DateOnly.FromDateTime(DateTime.Today.AddDays(30));

            var query = _repository.Query().AsNoTracking();

            // Expiring
            var expiring = await query
                .Where(x => x.ExpiryDate.HasValue
                         && x.ExpiryDate.Value >= today
                         && x.ExpiryDate.Value <= threshold
                         && x.Status.Id == ContractStatus.Active.Id)
                .OrderBy(x => x.ExpiryDate)
                .Select(ToSummaryResponse)
                .ToListAsync(ct);

            // Recently amended
            var recentlyAmended = await query
                .Where(x => x.Amendments.Any())
                .OrderByDescending(x => x.Amendments.Max(a => a.RecordedAtUtc))
                .Take(10)
                .Select(ToSummaryResponse)
                .ToListAsync(ct);

            // Stats (single query)
            var stats = await query
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Total = g.Count(),
                    Active = g.Count(x => x.Status.Id == ContractStatus.Active.Id),
                    Draft = g.Count(x => x.Status.Id == ContractStatus.Draft.Id),
                    Expired = g.Count(x => x.Status.Id == ContractStatus.Expired.Id),
                    Terminated = g.Count(x => x.Status.Id == ContractStatus.Terminated.Id),
                    Suspended = g.Count(x => x.Status.Id == ContractStatus.Suspended.Id)
                })
                .FirstOrDefaultAsync(ct);

            return new ContractReport
            {
                TotalContracts = stats?.Total ?? 0,
                ActiveContracts = stats?.Active ?? 0,
                DraftContracts = stats?.Draft ?? 0,
                ExpiredContracts = stats?.Expired ?? 0,
                TerminatedContracts = stats?.Terminated ?? 0,
                SuspendedContracts = stats?.Suspended ?? 0,
                ExpiringWithin30Days = expiring,
                RecentlyAmended = recentlyAmended
            };
        }

        // ══════════════════════════════════════════════════════════════════
        // Write — Pattern موحد: Load → Domain Call → Save
        // ══════════════════════════════════════════════════════════════════

        public async Task<Result> CreateAsync(string contractNumber, string type, DateOnly effectiveDate, DateOnly? expiryDate,
            string? notes, CancellationToken ct = default)
        {
            contractNumber = contractNumber.Trim();
            type = type.Trim();
            var exists = await _repository
                .Query()
                .AnyAsync(x => x.ContractNumber == contractNumber, ct);

            if (exists)
                return Result.Fail($"Contract number '{contractNumber}' already exists.");

            if (!ContractType.TryFromName(type, out var contractType))
                return Result.Fail($"Invalid contract type '{type}'.");

            if (contractType == null)
                return Result.Fail($"Invalid contract type '{type}'.");

            var contract = AContract.Create(contractNumber, contractType, effectiveDate, expiryDate, notes);
           

            await _repository.AddAsync(contract, ct);
            await _repository.SaveChangesAsync(ct);

            return Result.Ok();
        }

        public async Task<Result> ActivateAsync(int id, CancellationToken ct = default)
        {
            var result = await ExecuteOnContractAsync(id, c => c.Activate(DateOnly.FromDateTime(DateTime.Today)), ct);
            return result.ToContract();
        } 

        public async Task<Result> SuspendAsync(int id, string reason, CancellationToken ct = default)
        {
            var result = await ExecuteOnContractAsync(id, c => c.Suspend(reason), ct);
            return result.ToContract();
        }
           

        public async Task<Result> TerminateAsync(int id, string reason, CancellationToken ct = default)
        {
            var result = await ExecuteOnContractAsync(id, c => c.Terminate(reason), ct);
            return result.ToContract();
        } 

        public async Task<Result> MarkExpiredAsync(int id, CancellationToken ct = default)
        {
            var result = await ExecuteOnContractAsync(id, c => c.MarkExpired(DateOnly.FromDateTime(DateTime.Today)), ct);
            return result.ToContract();
        } 

        public async Task<Result> AddPartyAsync(int id, int companyId, string role, CancellationToken ct = default)
        {
            role = role?.Trim() ?? "";
            if (!Enum.TryParse<ContractRole>(role, ignoreCase: true, out var contractRole))
                return Result.Fail($"Invalid role '{role}'.");

            var result =  await ExecuteOnContractAsync(id, c => c.AddParty(companyId, contractRole), ct);
            return result.ToContract();
        }

        public async Task<Result> RemovePartyAsync(
            int id, int companyId, string role, CancellationToken ct = default)
        {
            if (!Enum.TryParse<ContractRole>(role, ignoreCase: true, out var contractRole))
                return Result.Fail($"Invalid role '{role}'.");

            var result = await ExecuteOnContractAsync(id, c => c.RemoveParty(companyId, contractRole), ct);
            return result.ToContract();
        }

        public async Task<Result> RemovePartyViaAmendmentAsync(
            int id, int companyId, string role, int amendmentNumber,
            CancellationToken ct = default)
        {
            if (!Enum.TryParse<ContractRole>(role, ignoreCase: true, out var contractRole))
                return Result.Fail($"Invalid role '{role}'.");

            var result = await ExecuteOnContractAsync(
                id,
                c => c.RemovePartyViaAmendment(companyId, contractRole, amendmentNumber),
                ct);

            return result.ToContract();
        }

        public async Task<Result> RecordAmendmentAsync(
            int id, string description, DateOnly effectiveDate,
            string changedBy, CancellationToken ct = default)
        {
            var recordedDate = DateOnly.FromDateTime(DateTime.Today);
            var recordedDateTimeUtc = DateTime.UtcNow;

            var result = await ExecuteOnContractAsync(
                id,
                c => c.RecordAmendment(description, effectiveDate, changedBy, recordedDate, recordedDateTimeUtc),
                ct
            );
            return result.ToContract();
        }

        public async Task<Result> ExtendExpiryAsync(
            int id, DateOnly newExpiryDate, int amendmentNumber,
            CancellationToken ct = default)
        {
            var result = await ExecuteOnContractAsync(
                id,
                c => c.ExtendExpiry(newExpiryDate, amendmentNumber, DateOnly.FromDateTime(DateTime.Today)),
                ct);

            return result.ToContract();
        }

        public async Task<Result> AttachFileAsync(
            int id, string fileUrl, string fileName,
            CancellationToken ct = default)
        {
            var contract = await _repository.GetByIdAsync(id, ct);
            if (contract is null)
                return Result.Fail($"Contract #{id} not found.");

            try { contract.AttachFile(fileUrl, fileName); }
            catch (ArgumentException ex) { return Result.Fail(ex.Message); }

            _repository.Update(contract);
            await _repository.SaveChangesAsync(ct);
            return Result.Ok();
        }

        // ══════════════════════════════════════════════════════════════════
        // Private Helper — يُلغي تكرار Load/Check/Save في كل دالة Write
        // ══════════════════════════════════════════════════════════════════

        private async Task<Domain.Common.Result> ExecuteOnContractAsync(
            int id,
            Func<AContract, Domain.Common.Result> domainAction,
            CancellationToken ct)
        {
            var contract = await _repository.GetByIdAsync(id, ct);
            if (contract is null)
                return Domain.Common.Result.Fail($"Contract #{id} not found.");

            var result = domainAction(contract);
            if (result.IsFailure)
                return result;

            _repository.Update(contract);
            await _repository.SaveChangesAsync(ct);
            return Domain.Common.Result.Ok();
        }

        // ══════════════════════════════════════════════════════════════════
        // Private Helpers
        // ══════════════════════════════════════════════════════════════════
        
        private Task<AContract?> GetOrFailAsync(int id, CancellationToken ct)
            => _repository.GetByIdAsync(id, ct);


        // ─── Party ────────────────────────────────────────────────────────
        // مُعاد الاستخدام داخل كل projection تحتاج Parties
        private static readonly Expression<Func<ContractParty, ContractPartyResponse>> ToPartyResponse =
            p => new ContractPartyResponse
            {
                CompanyId = p.CompanyId,
                Role = p.Role.ToString(),
            };

        // ─── Amendment ────────────────────────────────────────────────────
        private static readonly Expression<Func<ContractAmendment, ContractAmendmentResponse>> ToAmendmentResponse =
            a => new ContractAmendmentResponse
            {
                AmendmentNumber = a.AmendmentNumber,
                Description = a.Description,
                EffectiveDate = a.EffectiveDate,
                ChangedBy = a.ChangedBy,
                RecordedAtUtc = a.RecordedAtUtc
            };

        // ─── Summary ──────────────────────────────────────────────────────
        private static readonly Expression<Func<AContract, ContractSummary>> ToSummaryResponse =
            x => new ContractSummary
            {
                Id = x.Id,
                ContractNumber = x.ContractNumber,
                Type = x.Type.ToString(),
                Status = x.Status.Name,
                EffectiveDate = x.EffectiveDate,
                ExpiryDate = x.ExpiryDate,
                PartiesCount = x.Parties.Count,
                AmendmentsCount = x.Amendments.Count
            };

        // ─── Detail ───────────────────────────────────────────────────────
        private static readonly Expression<Func<AContract, ContractDetailResponse>> ToDetailResponse =
            x => new ContractDetailResponse
            {
                Id = x.Id,
                ContractNumber = x.ContractNumber,
                Type = x.Type.ToString(),
                Status = x.Status.Name,
                EffectiveDate = x.EffectiveDate,
                ExpiryDate = x.ExpiryDate,
                Notes = x.Notes,
                ContractFileUrl = x.ContractFileUrl,
                ContractFileName = x.ContractFileName,

                Parties = x.Parties.Select(p => new ContractPartyResponse
                {
                    CompanyId = p.CompanyId,
                    Role = p.Role.ToString(),
                }).ToList(),

                Amendments = x.Amendments
                    .OrderBy(a => a.AmendmentNumber)
                    .Select(a => new ContractAmendmentResponse
                    {
                        AmendmentNumber = a.AmendmentNumber,
                        Description = a.Description,
                        EffectiveDate = a.EffectiveDate,
                        ChangedBy = a.ChangedBy,
                        RecordedAtUtc = a.RecordedAtUtc
                    }).ToList()
            };


    }
}
