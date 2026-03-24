using Microsoft.EntityFrameworkCore;
using Marilog.Domain.Entities;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Application.Interfaces.Services;

namespace Marilog.Application.Services
{
    public class RankService : IRankService
    {
        private readonly IRepository<Rank> _repo;

        public RankService(IRepository<Rank> repo) => _repo = repo;

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<Rank?> GetByIdAsync(int id, CancellationToken ct = default)
            => await _repo.GetByIdAsync(id, ct);

        public async Task<Rank?> GetByCodeAsync(string code, CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .FirstOrDefaultAsync(x => x.RankCode == code.ToUpperInvariant(), ct);

        public async Task<IReadOnlyList<Rank>> GetAllAsync(CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .OrderBy(x => x.Department)
                          .ThenBy(x => x.RankName)
                          .ToListAsync(ct);

        public async Task<IReadOnlyList<Rank>> GetActiveAsync(CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .Where(x => x.IsActive)
                          .OrderBy(x => x.Department)
                          .ThenBy(x => x.RankName)
                          .ToListAsync(ct);

        public async Task<IReadOnlyList<Rank>> GetByDepartmentAsync(Department department,
            CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .Where(x => x.Department == department && x.IsActive)
                          .OrderBy(x => x.RankName)
                          .ToListAsync(ct);

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<Rank> CreateAsync(string rankCode, string rankName,
            Department department, CancellationToken ct = default)
        {
            var exists = await _repo.Query()
                .AnyAsync(x => x.RankCode == rankCode.ToUpperInvariant(), ct);
            if (exists)
                throw new InvalidOperationException($"Rank code '{rankCode}' already exists.");

            var rank = Rank.Create(rankCode, rankName, department);
            await _repo.AddAsync(rank, ct);
            await _repo.SaveChangesAsync(ct);
            return rank;
        }

        public async Task UpdateAsync(int id, string rankCode, string rankName,
            Department department, CancellationToken ct = default)
        {
            var rank = await GetOrThrowAsync(id, ct);

            var codeConflict = await _repo.Query()
                .AnyAsync(x => x.RankCode == rankCode.ToUpperInvariant()
                            && x.RankID   != id, ct);
            if (codeConflict)
                throw new InvalidOperationException($"Rank code '{rankCode}' is already in use.");

            rank.Update(rankCode, rankName, department);
            _repo.Update(rank);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task ActivateAsync(int id, CancellationToken ct = default)
        {
            var rank = await GetOrThrowAsync(id, ct);
            rank.Activate();
            _repo.Update(rank);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task DeactivateAsync(int id, CancellationToken ct = default)
        {
            var rank = await GetOrThrowAsync(id, ct);
            rank.Deactivate();
            _repo.Update(rank);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var rank = await GetOrThrowAsync(id, ct);
            _repo.HardDelete(rank);
            await _repo.SaveChangesAsync(ct);
        }

        // ── Private ───────────────────────────────────────────────────────────────

        private async Task<Rank> GetOrThrowAsync(int id, CancellationToken ct)
            => await _repo.GetByIdAsync(id, ct)
               ?? throw new KeyNotFoundException($"Rank {id} not found.");
    }
}
