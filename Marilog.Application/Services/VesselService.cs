using Microsoft.EntityFrameworkCore;
using Marilog.Domain.Entities;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Domain.Interfaces.Services;

namespace Marilog.Application.Services
{
    public class VesselService : IVesselService
    {
        private readonly IRepository<Vessel>  _repo;
        private readonly IRepository<Company> _companyRepo;

        public VesselService(
            IRepository<Vessel>  repo,
            IRepository<Company> companyRepo)
        {
            _repo        = repo;
            _companyRepo = companyRepo;
        }

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<Vessel?> GetByIdAsync(int id, CancellationToken ct = default)
            => await _repo.GetByIdAsync(id, ct);

        public async Task<Vessel?> GetByImoAsync(string imoNumber,
            CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .FirstOrDefaultAsync(x => x.IMONumber == imoNumber, ct);

        public async Task<IReadOnlyList<Vessel>> GetAllAsync(CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .Include(x => x.Company)
                          .OrderBy(x => x.VesselName)
                          .ToListAsync(ct);

        public async Task<IReadOnlyList<Vessel>> GetActiveAsync(CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .Where(x => x.IsActive)
                          .Include(x => x.Company)
                          .OrderBy(x => x.VesselName)
                          .ToListAsync(ct);

        public async Task<IReadOnlyList<Vessel>> GetByCompanyAsync(int companyId,
            CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .Where(x => x.CompanyID == companyId && x.IsActive)
                          .OrderBy(x => x.VesselName)
                          .ToListAsync(ct);

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<Vessel> CreateAsync(int companyId, string vesselName,
            string? imoNumber = null, decimal? grossTonnage = null,
            int? flagCountryId = null, string? notes = null,
            CancellationToken ct = default)
        {
            var companyExists = await _companyRepo.Query()
                .AnyAsync(x => x.CompanyID == companyId && x.IsActive, ct);
            if (!companyExists)
                throw new KeyNotFoundException($"Company {companyId} not found or inactive.");

            await EnsureUniqueImoAsync(imoNumber, excludeId: null, ct);

            var vessel = Vessel.Create(companyId, vesselName, imoNumber, grossTonnage,
                                       flagCountryId, notes);
            await _repo.AddAsync(vessel, ct);
            await _repo.SaveChangesAsync(ct);
            return vessel;
        }

        public async Task UpdateAsync(int id, string vesselName,
            string? imoNumber = null, decimal? grossTonnage = null,
            int? flagCountryId = null, string? notes = null,
            CancellationToken ct = default)
        {
            var vessel = await GetOrThrowAsync(id, ct);
            await EnsureUniqueImoAsync(imoNumber, excludeId: id, ct);

            vessel.Update(vesselName, imoNumber, grossTonnage, flagCountryId, notes);
            _repo.Update(vessel);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task ActivateAsync(int id, CancellationToken ct = default)
        {
            var vessel = await GetOrThrowAsync(id, ct);
            vessel.Activate();
            _repo.Update(vessel);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task DeactivateAsync(int id, CancellationToken ct = default)
        {
            var vessel = await GetOrThrowAsync(id, ct);
            vessel.Deactivate();
            _repo.Update(vessel);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var vessel = await GetOrThrowAsync(id, ct);

            var hasContracts = vessel.CrewContracts.Any();
            if (hasContracts)
                throw new InvalidOperationException(
                    "Cannot delete a vessel that has crew contracts. Deactivate it instead.");

            _repo.HardDelete(vessel);
            await _repo.SaveChangesAsync(ct);
        }

        // ── Private ───────────────────────────────────────────────────────────────

        private async Task<Vessel> GetOrThrowAsync(int id, CancellationToken ct)
            => await _repo.Query()
                          .Include(x => x.CrewContracts)
                          .FirstOrDefaultAsync(x => x.VesselID == id, ct)
               ?? throw new KeyNotFoundException($"Vessel {id} not found.");

        private async Task EnsureUniqueImoAsync(string? imoNumber,
            int? excludeId, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(imoNumber)) return;

            var conflict = await _repo.Query()
                .AnyAsync(x => x.IMONumber == imoNumber &&
                               (excludeId == null || x.VesselID != excludeId), ct);
            if (conflict)
                throw new InvalidOperationException(
                    $"IMO number '{imoNumber}' is already registered.");
        }
    }
}
