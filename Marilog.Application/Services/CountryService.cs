using Microsoft.EntityFrameworkCore;
using Marilog.Domain.Entities;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Application.Interfaces.Services;
using Marilog.Application.DTOs.Responses;

namespace Marilog.Application.Services
{
    public class CountryService : ICountryService
    {
        private readonly IRepository<Country> _repo;

        public CountryService(IRepository<Country> repo) => _repo = repo;

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<CountryResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _repo.Query()
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new CountryResponse
            {
                Id = x.Id,
                Code = x.CountryCode,
                Name = x.CountryName,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync(ct);
        }

        public async Task<CountryResponse?> GetByCodeAsync(string code, CancellationToken ct = default)
        {
            var upperCode = code.ToUpperInvariant();

            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.CountryCode == upperCode)
                .Select(x => new CountryResponse
                {
                    Id = x.Id,
                    Code = x.CountryCode,
                    Name = x.CountryName,
                    IsActive = x.IsActive
                })
                .FirstOrDefaultAsync(ct);
        }

        public async Task<IReadOnlyList<CountryResponse>> GetAllAsync(CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .OrderBy(x => x.CountryName)
                .Select(x => new CountryResponse
                {
                    Id = x.Id,
                    Code = x.CountryCode,
                    Name = x.CountryName,
                    IsActive = x.IsActive
                })
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<CountryResponse>> GetActiveAsync(CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.CountryName)
                .Select(x => new CountryResponse
                {
                    Id = x.Id,
                    Code = x.CountryCode,
                    Name = x.CountryName,
                    IsActive = x.IsActive
                })
                .ToListAsync(ct);
        }

        public async Task<bool> ExistsByCodeAsync(string code, CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .AnyAsync(x => x.CountryCode == code.ToUpperInvariant(), ct);

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<CountryResponse> CreateAsync(string countryCode, string countryName,
            CancellationToken ct = default)
        {
            if (await ExistsByCodeAsync(countryCode, ct))
                throw new InvalidOperationException($"Country code '{countryCode}' already exists.");

            var country = Country.Create(countryCode, countryName);
            await _repo.AddAsync(country, ct);
            await _repo.SaveChangesAsync(ct);
            return new CountryResponse
            {
                Id = country.Id,
                Code = country.CountryCode,
                Name = country.CountryName,
                IsActive = country.IsActive
            };
        }

        public async Task UpdateAsync(int id, string countryCode, string countryName,
            CancellationToken ct = default)
        {
            var country = await GetOrThrowAsync(id, ct);

            var codeConflict = await _repo.Query()
                .AnyAsync(x => x.CountryCode == countryCode.ToUpperInvariant()
                            && x.Id   != id, ct);
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
