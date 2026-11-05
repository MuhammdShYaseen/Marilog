using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Domain.Entities.SystemEntities;
using Marilog.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Marilog.Application.Services.ApplicationServices.SystemServices
{
    public class ContractTypeService : IContractTypeService
    {
        private readonly IRepository<ContractType> _repo;
        private readonly IRepository<AContract> _contracts;

        public ContractTypeService(IRepository<ContractType> repo, IRepository<AContract> contracts)
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
                throw new InvalidOperationException($"Contract type code '{upper}' already exists.");

            var type = ContractType.Create(code, name, sortOrder);
            await _repo.AddAsync(type, ct);
            await _repo.SaveChangesAsync(ct);

            return new ContractLookupResponse
            {
                Id = type.Id,
                Code = type.Code,
                Name = type.Name,
                SortOrder = type.SortOrder,
                IsActive = type.IsActive
            };
        }

        public async Task UpdateAsync(int id, string name, int sortOrder, CancellationToken ct = default)
        {
            var type = await GetOrThrowAsync(id, ct);
            type.Update(name, sortOrder);
            _repo.Update(type);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task ActivateAsync(int id, CancellationToken ct = default)
        {
            var type = await GetOrThrowAsync(id, ct);
            type.Activate();
            _repo.Update(type);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task DeactivateAsync(int id, CancellationToken ct = default)
        {
            var type = await GetOrThrowAsync(id, ct);
            type.Deactivate();
            _repo.Update(type);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var type = await GetOrThrowAsync(id, ct);

            if (await _contracts.Query().AnyAsync(x => x.ContractTypeId == id, ct))
                throw new InvalidOperationException(
                    $"Contract type '{type.Name}' is used by existing contracts. Deactivate it instead.");

            _repo.HardDelete(type);
            await _repo.SaveChangesAsync(ct);
        }

        // ── Private ───────────────────────────────────────────────────────────────

        private async Task<ContractType> GetOrThrowAsync(int id, CancellationToken ct)
            => await _repo.GetByIdAsync(id, ct)
               ?? throw new KeyNotFoundException($"Contract type {id} not found.");

        private static readonly Expression<Func<ContractType, ContractLookupResponse>> ToResponse =
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
