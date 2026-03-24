using Microsoft.EntityFrameworkCore;
using Marilog.Domain.Entities;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Application.Interfaces.Services;

namespace Marilog.Application.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly IRepository<Company> _repo;

        public CompanyService(IRepository<Company> repo) => _repo = repo;

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<Company?> GetByIdAsync(int id, CancellationToken ct = default)
            => await _repo.GetByIdAsync(id, ct);

        public async Task<Company?> GetWithVesselsAsync(int id, CancellationToken ct = default)
            => await _repo.Query()
                          .AsNoTracking()
                          .Include(x => x.Vessels)
                          .FirstOrDefaultAsync(x => x.CompanyID == id, ct);

        public async Task<IReadOnlyList<Company>> GetAllAsync(CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .OrderBy(x => x.CompanyName)
                          .ToListAsync(ct);

        public async Task<IReadOnlyList<Company>> GetActiveAsync(CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .Where(x => x.IsActive)
                          .OrderBy(x => x.CompanyName)
                          .ToListAsync(ct);

        public async Task<IReadOnlyList<Company>> SearchByNameAsync(string name,
            CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .Where(x => x.IsActive &&
                                      x.CompanyName.Contains(name))
                          .OrderBy(x => x.CompanyName)
                          .ToListAsync(ct);

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<Company> CreateAsync(string companyName, int? countryId = null,
            string? contactName = null, string? email = null,
            string? phone = null, string? address = null,
            CancellationToken ct = default)
        {
            var company = Company.Create(companyName, countryId, contactName, email, phone, address);
            await _repo.AddAsync(company, ct);
            await _repo.SaveChangesAsync(ct);
            return company;
        }

        public async Task UpdateAsync(int id, string companyName, int? countryId = null,
            string? contactName = null, string? email = null,
            string? phone = null, string? address = null,
            CancellationToken ct = default)
        {
            var company = await GetOrThrowAsync(id, ct);
            company.Update(companyName, countryId, contactName, email, phone, address);
            _repo.Update(company);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task ActivateAsync(int id, CancellationToken ct = default)
        {
            var company = await GetOrThrowAsync(id, ct);
            company.Activate();
            _repo.Update(company);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task DeactivateAsync(int id, CancellationToken ct = default)
        {
            var company = await GetOrThrowAsync(id, ct);
            company.Deactivate();
            _repo.Update(company);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var company = await GetOrThrowAsync(id, ct);

            var hasVessels = await _repo.Query()
                .Where(x => x.CompanyID == id)
                .SelectMany(x => x.Vessels)
                .AnyAsync(ct);
            if (hasVessels)
                throw new InvalidOperationException(
                    "Cannot delete a company that has vessels. Deactivate it instead.");

            _repo.HardDelete(company);
            await _repo.SaveChangesAsync(ct);
        }

        // ── Private ───────────────────────────────────────────────────────────────

        private async Task<Company> GetOrThrowAsync(int id, CancellationToken ct)
            => await _repo.GetByIdAsync(id, ct)
               ?? throw new KeyNotFoundException($"Company {id} not found.");
    }
}
