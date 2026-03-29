using Marilog.Application.DTOs.Responses;
using Marilog.Application.Interfaces.Services;
using Marilog.Domain.Entities;
using Marilog.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Marilog.Application.Services
{
    public class RankService : IRankService
    {
        private readonly IRepository<Rank> _repo;

        public RankService(IRepository<Rank> repo) => _repo = repo;

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<RankResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(ToResponse)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<RankResponse?> GetByCodeAsync(string code, CancellationToken ct = default)
        {
            var upper = code.ToUpperInvariant();

            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.RankCode == upper)
                .Select(ToResponse)
                .FirstOrDefaultAsync(ct);
        }
        public async Task<IReadOnlyList<RankResponse>> GetAllAsync(CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .OrderBy(x => x.Department)
                .ThenBy(x => x.RankName)
                .Select(ToResponse)
                .ToListAsync(ct);
        }
        public async Task<IReadOnlyList<RankResponse>> GetActiveAsync(CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Department)
                .ThenBy(x => x.RankName)
                .Select(ToResponse)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<RankResponse>> GetByDepartmentAsync(Department department,
            CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.Department == department && x.IsActive)
                .OrderBy(x => x.RankName)
                .Select(ToResponse)
                .ToListAsync(ct);
        }

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<RankResponse> CreateAsync(string rankCode, string rankName,
            Department department, CancellationToken ct = default)
        {
            var exists = await _repo.Query()
                .AnyAsync(x => x.RankCode == rankCode.ToUpperInvariant(), ct);
            if (exists)
                throw new InvalidOperationException($"Rank code '{rankCode}' already exists.");

            var rank = Rank.Create(rankCode, rankName, department);
            await _repo.AddAsync(rank, ct);
            await _repo.SaveChangesAsync(ct);
            return new RankResponse
            {
                Code = rankCode,
                Department = (int)department,
                IsActive = true,
                Name = rankName,
            };
        }

        public async Task UpdateAsync(int id, string rankCode, string rankName,
            Department department, CancellationToken ct = default)
        {
            var rank = await GetOrThrowAsync(id, ct);

            var codeConflict = await _repo.Query()
                .AnyAsync(x => x.RankCode == rankCode.ToUpperInvariant()
                            && x.Id != id, ct);
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

        private static readonly Expression<Func<Rank, RankResponse>> ToResponse =
        x => new RankResponse
        {
            Id = x.Id,
            Code = x.RankCode,
            Name = x.RankName,
            Department = (int)x.Department,
            IsActive = x.IsActive
        };
    }
}
