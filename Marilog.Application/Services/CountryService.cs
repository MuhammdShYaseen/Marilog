using Microsoft.EntityFrameworkCore;
using Marilog.Domain.Entities;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Domain.Interfaces.Services;

namespace Marilog.Application.Services
{
    public class CountryService : ICountryService
    {
        private readonly IRepository<Country> _repo;

        public CountryService(IRepository<Country> repo) => _repo = repo;

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<Country?> GetByIdAsync(int id, CancellationToken ct = default)
            => await _repo.GetByIdAsync(id, ct);

        public async Task<Country?> GetByCodeAsync(string code, CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .FirstOrDefaultAsync(x => x.CountryCode == code.ToUpperInvariant(), ct);

        public async Task<IReadOnlyList<Country>> GetAllAsync(CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .OrderBy(x => x.CountryName)
                          .ToListAsync(ct);

        public async Task<IReadOnlyList<Country>> GetActiveAsync(CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .Where(x => x.IsActive)
                          .OrderBy(x => x.CountryName)
                          .ToListAsync(ct);

        public async Task<bool> ExistsByCodeAsync(string code, CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .AnyAsync(x => x.CountryCode == code.ToUpperInvariant(), ct);

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<Country> CreateAsync(string countryCode, string countryName,
            CancellationToken ct = default)
        {
            if (await ExistsByCodeAsync(countryCode, ct))
                throw new InvalidOperationException($"Country code '{countryCode}' already exists.");

            var country = Country.Create(countryCode, countryName);
            await _repo.AddAsync(country, ct);
            await _repo.SaveChangesAsync(ct);
            return country;
        }

        public async Task UpdateAsync(int id, string countryCode, string countryName,
            CancellationToken ct = default)
        {
            var country = await GetOrThrowAsync(id, ct);

            var codeConflict = await _repo.Query()
                .AnyAsync(x => x.CountryCode == countryCode.ToUpperInvariant()
                            && x.CountryID   != id, ct);
            if (codeConflict)
                throw new InvalidOperationException($"Country code '{countryCode}' is already in use.");

            country.Update(countryCode, countryName);
            _repo.Update(country);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task ActivateAsync(int id, CancellationToken ct = default)
        {
            var country = await GetOrThrowAsync(id, ct);
            country.Activate();
            _repo.Update(country);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task DeactivateAsync(int id, CancellationToken ct = default)
        {
            var country = await GetOrThrowAsync(id, ct);
            country.Deactivate();
            _repo.Update(country);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var country = await GetOrThrowAsync(id, ct);
            _repo.HardDelete(country);
            await _repo.SaveChangesAsync(ct);
        }

        // ── Private ───────────────────────────────────────────────────────────────

        private async Task<Country> GetOrThrowAsync(int id, CancellationToken ct)
            => await _repo.GetByIdAsync(id, ct)
               ?? throw new KeyNotFoundException($"Country {id} not found.");
    }
}
