using Marilog.Domain.Interfaces.Repositories;
using Marilog.Ship.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Marilog.Ship.Services.DeductionTypes
{
    public class DeductionTypeService : IDeductionTypeService
    {
        private readonly IRepository<DeductionType> _repo;

        public DeductionTypeService(IRepository<DeductionType> repo) => _repo = repo;

        public async Task<IReadOnlyList<DeductionTypeResponse>> GetAllAsync(CancellationToken ct = default) =>
            await _repo.Query().AsNoTracking()
                .OrderBy(x => x.Name)
                .Select(x => new DeductionTypeResponse(x.Id, x.Code, x.Name, x.IsActive))
                .ToListAsync(ct);

        public async Task<IReadOnlyList<DeductionTypeResponse>> GetActiveAsync(CancellationToken ct = default) =>
            await _repo.Query().AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .Select(x => new DeductionTypeResponse(x.Id, x.Code, x.Name, x.IsActive))
                .ToListAsync(ct);

        public async Task CreateAsync(string code, string name, CancellationToken ct = default)
        {
            var type = DeductionType.Create(code, name);

            if (await _repo.Query().IgnoreQueryFilters().AnyAsync(x => x.Code == type.Code, ct))
                throw new InvalidOperationException($"The code '{type.Code}' is already used.");

            await _repo.AddAsync(type, ct);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(int id, string name, CancellationToken ct = default)
        {
            var type = await GetOrThrowAsync(id, ct);
            type.Update(name);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task SetActiveAsync(int id, bool isActive, CancellationToken ct = default)
        {
            var type = await GetOrThrowAsync(id, ct);
            if (isActive) type.Activate(); else type.Deactivate();
            await _repo.SaveChangesAsync(ct);
        }

        private async Task<DeductionType> GetOrThrowAsync(int id, CancellationToken ct) =>
            await _repo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Deduction type {id} not found.");
    }
}