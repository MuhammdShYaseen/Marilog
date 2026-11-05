using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Domain.Entities.SystemEntities;
using Marilog.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Marilog.Application.Services.ApplicationServices.SystemServices
{
    public class ContractRoleService : IContractRoleService
    {
        private readonly IRepository<ContractRole> _repo;
        private readonly IRepository<AContract> _contracts;

        public ContractRoleService(IRepository<ContractRole> repo, IRepository<AContract> contracts)
        {
            _repo = repo;
            _contracts = contracts;
        }

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<IReadOnlyList<ContractLookupResponse>> GetAllAsync(CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                .OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
                .Select(ToResponse)
                .ToListAsync(ct);

        public async Task<IReadOnlyList<ContractLookupResponse>> GetActiveAsync(CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
                .Select(ToResponse)
                .ToListAsync(ct);

        public async Task<ContractLookupResponse?> GetByIdAsync(int id, CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                .Where(x => x.Id == id)
                .Select(ToResponse)
                .FirstOrDefaultAsync(ct);

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<ContractLookupResponse> CreateAsync(string code, string name, int sortOrder = 0, CancellationToken ct = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(code);
            var upper = code.Trim().ToUpperInvariant();

            if (await _repo.Query().AnyAsync(x => x.Code == upper, ct))
                throw new InvalidOperationException($"Contract role code '{upper}' already exists.");

            var role = ContractRole.Create(code, name, sortOrder);
            await _repo.AddAsync(role, ct);
            await _repo.SaveChangesAsync(ct);

            return new ContractLookupResponse
            {
                Id = role.Id,
                Code = role.Code,
                Name = role.Name,
                SortOrder = role.SortOrder,
                IsActive = role.IsActive
            };
        }

        public async Task UpdateAsync(int id, string name, int sortOrder, CancellationToken ct = default)
        {
            var role = await GetOrThrowAsync(id, ct);
            role.Update(name, sortOrder);
            _repo.Update(role);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task ActivateAsync(int id, CancellationToken ct = default)
        {
            var role = await GetOrThrowAsync(id, ct);
            role.Activate();
            _repo.Update(role);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task DeactivateAsync(int id, CancellationToken ct = default)
        {
            var role = await GetOrThrowAsync(id, ct);
            role.Deactivate();
            _repo.Update(role);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var role = await GetOrThrowAsync(id, ct);

            if (await _contracts.Query().AnyAsync(x => x.Parties.Any(p => p.RoleId == id), ct))
                throw new InvalidOperationException(
                    $"Role '{role.Name}' is used in existing contracts. Deactivate it instead.");

            _repo.HardDelete(role);
            await _repo.SaveChangesAsync(ct);
        }

        // ── Private ───────────────────────────────────────────────────────────────

        private async Task<ContractRole> GetOrThrowAsync(int id, CancellationToken ct)
            => await _repo.GetByIdAsync(id, ct)
               ?? throw new KeyNotFoundException($"Contract role {id} not found.");

        private static readonly Expression<Func<ContractRole, ContractLookupResponse>> ToResponse =
            x => new ContractLookupResponse
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive
            };
    }
}
