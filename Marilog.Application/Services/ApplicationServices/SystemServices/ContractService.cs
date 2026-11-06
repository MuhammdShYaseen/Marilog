using Marilog.Application.Extensions;
using Marilog.Application.Interfaces.UserIdentity;
using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Reports.Contract;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Domain.Entities.SystemEntities;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Kernel.Enumerations;
using Marilog.Kernel.Primitives;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Marilog.Application.Services.ApplicationServices.SystemServices
{
    public class ContractService : IContractService
    {
        private readonly IRepository<AContract> _repository;
        private readonly IRepository<Domain.Entities.SystemEntities.ContractType> _contractTypes;
        private readonly IRepository<ContractRole> _contractRoles;
        private readonly IRepository<Company> _companies;
        private readonly ICurrentUserService _currentUser;
        private readonly IRepository<Voyage> _voyages;
        public ContractService(
            IRepository<AContract> repository,
            IRepository<ContractType> contractTypes,
            IRepository<ContractRole> contractRoles,
            IRepository<Company> companies,
            IRepository<Voyage> voyages,
            ICurrentUserService currentUser)
        {
            _repository = repository;
            _contractTypes = contractTypes;
            _contractRoles = contractRoles;
            _companies = companies;
            _voyages = voyages;
            _currentUser = currentUser;
        }

        private string ChangedBy => _currentUser.UserName ?? "system";
        private static DateOnly Today => DateOnly.FromDateTime(DateTime.Today);

        // ══════════════════════════════════════════════════════════════════
        // Queries
        // ══════════════════════════════════════════════════════════════════

        public async Task<ContractDetailResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var contract = await _repository.Query()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(ToDetailResponse)
                .FirstOrDefaultAsync(ct);

            if (contract is not null)
                await FillPartyNamesAsync(contract.Parties, ct);

            return contract;
        }

        public async Task<ContractDetailResponse?> GetByNumberAsync(string number, CancellationToken ct = default)
        {
            var id = await _repository.Query()
                .AsNoTracking()
                .Where(x => x.ContractNumber == number)
                .Select(x => (int?)x.Id)
                .FirstOrDefaultAsync(ct);

            return id is null ? null : await GetByIdAsync(id.Value, ct);
        }

        public async Task<PagedResponse<ContractSummary>> GetPagedAsync(ContractFilter filter, CancellationToken ct = default)
        {
            var page = Math.Max(filter.Page, 1);
            var pageSize = Math.Min(Math.Max(filter.PageSize, 1), 100);

            var query = _repository.Query()
                .AsNoTracking()
                .ApplyFilter(filter);

            var total = await query.CountAsync(ct);

            var items = await query
                .ApplySort(filter)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(ToSummaryResponse)
                .ToListAsync(ct);

            await FillPartyNamesAsync(items.SelectMany(x => x.Parties), ct);

            return new PagedResponse<ContractSummary>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<List<ContractSummary>> GetExpiringAsync(int withinDays, CancellationToken ct = default)
        {
            var threshold = Today.AddDays(withinDays);

            var items = await _repository.Query()
                .AsNoTracking()
                .Where(x => x.ExpiryDate.HasValue
                         && x.ExpiryDate.Value <= threshold
                         && x.Status == ContractStatus.Active)
                .OrderBy(x => x.ExpiryDate)
                .Select(ToSummaryResponse)
                .ToListAsync(ct);

            await FillPartyNamesAsync(items.SelectMany(x => x.Parties), ct);
            return items;
        }

        public async Task<ContractReport> GetReportAsync(CancellationToken ct = default)
        {
            var query = _repository.Query().AsNoTracking();

            var expiring = await GetExpiringAsync(30, ct);

            var recentlyAmended = await query
                .Where(x => x.Amendments.Any())
                .OrderByDescending(x => x.Amendments.Max(a => a.RecordedAtUtc))
                .Take(10)
                .Select(ToSummaryResponse)
                .ToListAsync(ct);

            await FillPartyNamesAsync(recentlyAmended.SelectMany(x => x.Parties), ct);

            var stats = await query
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Total = g.Count(),
                    Active = g.Count(x => x.Status == ContractStatus.Active),
                    Draft = g.Count(x => x.Status == ContractStatus.Draft),
                    Expired = g.Count(x => x.Status == ContractStatus.Expired),
                    Terminated = g.Count(x => x.Status == ContractStatus.Terminated),
                    Suspended = g.Count(x => x.Status == ContractStatus.Suspended)
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
        // Write
        // ══════════════════════════════════════════════════════════════════

        public async Task<Result> CreateAsync(string contractNumber, int contractTypeId, DateOnly effectiveDate,
            DateOnly? expiryDate, string? notes, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(contractNumber))
                return Result.Fail("Contract number is required.");

            contractNumber = contractNumber.Trim();

            if (await _repository.Query().AnyAsync(x => x.ContractNumber == contractNumber, ct))
                return Result.Fail($"Contract number '{contractNumber}' already exists.");

            if (!await _contractTypes.Query().AnyAsync(x => x.Id == contractTypeId && x.IsActive, ct))
                return Result.Fail("Contract type not found or inactive.");

            var contract = AContract.Create(contractNumber, contractTypeId, effectiveDate, expiryDate, notes);

            await _repository.AddAsync(contract, ct);
            await _repository.SaveChangesAsync(ct);
            return Result.Ok();
        }

        public async Task<Result> UpdateAsync(int id, string contractNumber, int contractTypeId, DateOnly effectiveDate,
            DateOnly? expiryDate, string? notes, CancellationToken ct = default)
        {
            var contract = await _repository.GetByIdAsync(id, ct);
            if (contract is null)
                return Result.Fail($"Contract #{id} not found.");

            var number = contractNumber?.Trim() ?? string.Empty;
            if (await _repository.Query().AnyAsync(x => x.Id != id && x.ContractNumber == number, ct))
                return Result.Fail($"Contract number '{number}' already exists.");

            // An inactive type is accepted only if the contract already has it.
            if (contractTypeId != contract.ContractTypeId &&
                !await _contractTypes.Query().AnyAsync(x => x.Id == contractTypeId && x.IsActive, ct))
                return Result.Fail("Contract type not found or inactive.");

            var result = contract.Update(number, contractTypeId, effectiveDate, expiryDate, notes);
            if (result.IsFailure)
                return result;

            _repository.Update(contract);
            await _repository.SaveChangesAsync(ct);
            return Result.Ok();
        }

        /// <summary>Only Draft contracts without charter terms can be deleted.</summary>
        public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
        {
            var contract = await _repository.GetByIdAsync(id, ct);
            if (contract is null)
                return Result.Fail($"Contract #{id} not found.");

            if (contract.Status != ContractStatus.Draft)
                return Result.Fail("Only Draft contracts can be deleted. Terminate it instead.");

            var hasCharterTerms = await _repository.Query()
                .Where(x => x.Id == id)
                .Select(x => x.CharterTerms != null)
                .FirstOrDefaultAsync(ct);

            if (hasCharterTerms)
                return Result.Fail("This contract has charter terms (laytime). It cannot be deleted.");

            var linkedToVoyage = await _voyages.Query()
                .IgnoreQueryFilters()
                .AnyAsync(v => v.Contracts.Any(c => c.ContractID == id), ct);

            if (linkedToVoyage)
                return Result.Fail("This contract is linked to a voyage. Unlink it first.");


            _repository.HardDelete(contract);
            await _repository.SaveChangesAsync(ct);
            return Result.Ok();
        }

        public Task<Result> ActivateAsync(int id, CancellationToken ct = default)
            => ExecuteOnContractAsync(id, c => c.Activate(Today), ct);

        public Task<Result> SuspendAsync(int id, string reason, CancellationToken ct = default)
            => ExecuteOnContractAsync(id, c => c.Suspend(reason), ct);

        public Task<Result> TerminateAsync(int id, string reason, CancellationToken ct = default)
            => ExecuteOnContractAsync(id, c => c.Terminate(reason), ct);

        public Task<Result> MarkExpiredAsync(int id, CancellationToken ct = default)
            => ExecuteOnContractAsync(id, c => c.MarkExpired(Today), ct);

        public async Task<Result> AddPartyAsync(int id, int companyId, int roleId, CancellationToken ct = default)
        {
            var companyName = await GetCompanyNameAsync(companyId, ct);
            if (companyName is null)
                return Result.Fail($"Company #{companyId} not found.");

            var roleName = await _contractRoles.Query()
                .AsNoTracking()
                .Where(r => r.Id == roleId && r.IsActive)
                .Select(r => r.Name)
                .FirstOrDefaultAsync(ct);

            if (roleName is null)
                return Result.Fail("Role not found or inactive.");

            return await ExecuteOnContractAsync(id,
                c => c.AddParty(companyId, companyName, roleId, roleName, ChangedBy, DateTime.UtcNow), ct);
        }

        public async Task<Result> RemovePartyAsync(int id, int companyId, int roleId, CancellationToken ct = default)
        {
            var companyName = await GetCompanyNameAsync(companyId, ct) ?? $"Company #{companyId}";

            var roleName = await _contractRoles.Query()
                .AsNoTracking()
                .Where(r => r.Id == roleId)
                .Select(r => r.Name)
                .FirstOrDefaultAsync(ct) ?? $"Role #{roleId}";

            return await ExecuteOnContractAsync(id,
                c => c.RemoveParty(companyId, companyName, roleId, roleName, ChangedBy, DateTime.UtcNow), ct);
        }

        public Task<Result> RecordAmendmentAsync(int id, string description, DateOnly effectiveDate, CancellationToken ct = default)
            => ExecuteOnContractAsync(id, c => c.RecordAmendment(description, effectiveDate, ChangedBy, DateTime.UtcNow), ct);

        public Task<Result> ChangeExpiryAsync(int id, DateOnly newExpiryDate, CancellationToken ct = default)
            => ExecuteOnContractAsync(id, c => c.ChangeExpiry(newExpiryDate, ChangedBy, DateTime.UtcNow), ct);

        // ══════════════════════════════════════════════════════════════════
        // Private
        // ══════════════════════════════════════════════════════════════════

        private async Task<Result> ExecuteOnContractAsync(int id, Func<AContract, Result> domainAction, CancellationToken ct)
        {
            var contract = await _repository.GetByIdAsync(id, ct);
            if (contract is null)
                return Result.Fail($"Contract #{id} not found.");

            var result = domainAction(contract);
            if (result.IsFailure)
                return result;

            _repository.Update(contract);
            await _repository.SaveChangesAsync(ct);
            return Result.Ok();
        }

        private Task<string?> GetCompanyNameAsync(int companyId, CancellationToken ct)
            => _companies.Query()
                .AsNoTracking()
                .Where(c => c.Id == companyId)
                .Select(c => c.CompanyName)
                .FirstOrDefaultAsync(ct);

        // ContractParty holds only CompanyId + RoleId — names are filled with two small queries.
        private async Task FillPartyNamesAsync(IEnumerable<ContractPartyResponse> parties, CancellationToken ct)
        {
            var list = parties.ToList();
            if (list.Count == 0)
                return;

            var companyIds = list.Select(p => p.CompanyId).Distinct().ToList();
            var roleIds = list.Select(p => p.RoleId).Distinct().ToList();

            var companies = await _companies.Query()
                .AsNoTracking()
                .Where(c => companyIds.Contains(c.Id))
                .Select(c => new { c.Id, c.CompanyName })
                .ToDictionaryAsync(c => c.Id, c => c.CompanyName, ct);

            var roles = await _contractRoles.Query()
                .AsNoTracking()
                .Where(r => roleIds.Contains(r.Id))
                .Select(r => new { r.Id, r.Name })
                .ToDictionaryAsync(r => r.Id, r => r.Name, ct);

            foreach (var p in list)
            {
                p.CompanyName = companies.GetValueOrDefault(p.CompanyId) ?? $"Company #{p.CompanyId}";
                p.Role = roles.GetValueOrDefault(p.RoleId) ?? $"Role #{p.RoleId}";
            }
        }

        // ─── Projections ──────────────────────────────────────────────────

        private static readonly Expression<Func<AContract, ContractSummary>> ToSummaryResponse =
            x => new ContractSummary
            {
                Id = x.Id,
                ContractNumber = x.ContractNumber,
                ContractTypeId = x.ContractTypeId,
                Type = x.ContractType.Name,
                Status = x.Status.Name,
                EffectiveDate = x.EffectiveDate,
                ExpiryDate = x.ExpiryDate,
                AmendmentsCount = x.Amendments.Count,
                Parties = x.Parties
                    .Select(p => new ContractPartyResponse { CompanyId = p.CompanyId, RoleId = p.RoleId })
                    .ToList()
            };

        private static readonly Expression<Func<AContract, ContractDetailResponse>> ToDetailResponse =
            x => new ContractDetailResponse
            {
                Id = x.Id,
                ContractNumber = x.ContractNumber,
                ContractTypeId = x.ContractTypeId,
                Type = x.ContractType.Name,
                Status = x.Status.Name,
                EffectiveDate = x.EffectiveDate,
                ExpiryDate = x.ExpiryDate,
                Notes = x.Notes,
                HasCharterTerms = x.CharterTerms != null,
                Parties = x.Parties
                    .Select(p => new ContractPartyResponse { CompanyId = p.CompanyId, RoleId = p.RoleId })
                    .ToList(),
                Amendments = x.Amendments
                    .OrderBy(a => a.AmendmentNumber)
                    .Select(a => new ContractAmendmentResponse
                    {
                        AmendmentNumber = a.AmendmentNumber,
                        Description = a.Description,
                        EffectiveDate = a.EffectiveDate,
                        ChangedBy = a.ChangedBy,
                        RecordedAtUtc = a.RecordedAtUtc
                    })
                    .ToList()
            };
    }
}
