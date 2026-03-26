using Marilog.Application.DTOs;
using Marilog.Application.Interfaces.Services;
using Marilog.Domain.Entities;
using Marilog.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

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

        public async Task<VesselResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(ToResponse)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<VesselResponse?> GetByImoAsync(string imoNumber,
            CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.IMONumber == imoNumber)
                .Select(ToResponse)
                .FirstOrDefaultAsync(ct);
        }
        public async Task<IReadOnlyList<VesselResponse>> GetAllAsync(CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .OrderBy(x => x.VesselName)
                .Select(ToResponse)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<VesselResponse>> GetActiveAsync(CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.VesselName)
                .Select(ToResponse)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<VesselResponse>> GetByCompanyAsync(int companyId,
            CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.CompanyID == companyId && x.IsActive)
                .OrderBy(x => x.VesselName)
                .Select(ToResponse)
                .ToListAsync(ct);
        }

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<VesselResponse> CreateAsync(int companyId, string vesselName,
            string? imoNumber = null, decimal? grossTonnage = null,
            int? flagCountryId = null, string? notes = null,
            CancellationToken ct = default)
        {
            var companyExists = await _companyRepo.Query()
                .AnyAsync(x => x.Id == companyId && x.IsActive, ct);
            if (!companyExists)
                throw new KeyNotFoundException($"Company {companyId} not found or inactive.");

            await EnsureUniqueImoAsync(imoNumber, excludeId: null, ct);

            var vessel = Vessel.Create(companyId, vesselName, imoNumber, grossTonnage,
                                       flagCountryId, notes);
            await _repo.AddAsync(vessel, ct);
            await _repo.SaveChangesAsync(ct);
            return new VesselResponse
            {
                CompanyId = companyId,
                Name = vesselName,
                FlagCountryId = flagCountryId,
                IMONumber = imoNumber,
                GrossTonnage = grossTonnage,
            };
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
                          .FirstOrDefaultAsync(x => x.Id == id, ct)
               ?? throw new KeyNotFoundException($"Vessel {id} not found.");

        private async Task EnsureUniqueImoAsync(string? imoNumber,
            int? excludeId, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(imoNumber)) return;

            var conflict = await _repo.Query()
                .AnyAsync(x => x.IMONumber == imoNumber &&
                               (excludeId == null || x.Id != excludeId), ct);
            if (conflict)
                throw new InvalidOperationException(
                    $"IMO number '{imoNumber}' is already registered.");
        }

        private static readonly Expression<Func<Vessel, VesselResponse>> ToResponse =
        x => new VesselResponse
        {
            Id = x.Id,
            Name = x.VesselName,
            IMONumber = x.IMONumber,
            GrossTonnage = x.GrossTonnage,
            Notes = x.Notes,

            CompanyId = x.CompanyID,
            CompanyName = x.Company != null
                ? x.Company.CompanyName
                : null,

            FlagCountryId = x.FlagCountryID,
            FlagCountryName = x.FlagCountry != null
                ? x.FlagCountry.CountryName
                : null,

            IsActive = x.IsActive
        };
    }
}
