using Microsoft.EntityFrameworkCore;
using Marilog.Domain.Entities;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Domain.Interfaces.Services;

namespace Marilog.Application.Services
{
    public class OfficeService : IOfficeService
    {
        private readonly IRepository<Office> _repo;

        public OfficeService(IRepository<Office> repo) => _repo = repo;

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<Office?> GetByIdAsync(int id, CancellationToken ct = default)
            => await _repo.GetByIdAsync(id, ct);

        public async Task<IReadOnlyList<Office>> GetAllAsync(CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .OrderBy(x => x.OfficeName)
                          .ToListAsync(ct);

        public async Task<IReadOnlyList<Office>> GetActiveAsync(CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .Where(x => x.IsActive)
                          .OrderBy(x => x.OfficeName)
                          .ToListAsync(ct);

        public async Task<IReadOnlyList<Office>> GetByCountryAsync(int countryId,
            CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .Where(x => x.CountryId == countryId && x.IsActive)
                          .OrderBy(x => x.City)
                          .ThenBy(x => x.OfficeName)
                          .ToListAsync(ct);

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<Office> CreateAsync(string officeName, string city, int countryId,
            string? address = null, string? phone = null, string? contactName = null,
            CancellationToken ct = default)
        {
            var office = Office.Create(officeName, city, countryId, address, phone, contactName);
            await _repo.AddAsync(office, ct);
            await _repo.SaveChangesAsync(ct);
            return office;
        }

        public async Task UpdateAsync(int id, string officeName, string city, int countryId,
            string? address = null, string? phone = null, string? contactName = null,
            CancellationToken ct = default)
        {
            var office = await GetOrThrowAsync(id, ct);
            office.Update(officeName, city, countryId, address, phone, contactName);
            _repo.Update(office);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task ActivateAsync(int id, CancellationToken ct = default)
        {
            var office = await GetOrThrowAsync(id, ct);
            office.Activate();
            _repo.Update(office);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task DeactivateAsync(int id, CancellationToken ct = default)
        {
            var office = await GetOrThrowAsync(id, ct);
            office.Deactivate();
            _repo.Update(office);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var office = await GetOrThrowAsync(id, ct);
            _repo.HardDelete(office);
            await _repo.SaveChangesAsync(ct);
        }

        // ── Private ───────────────────────────────────────────────────────────────

        private async Task<Office> GetOrThrowAsync(int id, CancellationToken ct)
            => await _repo.GetByIdAsync(id, ct)
               ?? throw new KeyNotFoundException($"Office {id} not found.");
    }
}
