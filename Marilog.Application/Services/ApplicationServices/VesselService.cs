
using Marilog.Contracts.DTOs.Requests.VesselDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services;
using Marilog.Domain.Entities.SystemEntities;
using Marilog.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Marilog.Application.Services.ApplicationServices
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

        // Application/Vessels/Services/VesselService.cs

        public async Task<IReadOnlyList<VesselResponse>> CreateRangeAsync(
            IEnumerable<CreateVesselRequest> commands,
            CancellationToken ct = default)
        {
            var commandList = commands.ToList();
            if (!commandList.Any())
                return Array.Empty<VesselResponse>();

            // --- تحقق من أن جميع الـ CompanyIds موجودة ونشطة بـ query واحدة ---
            var companyIds = commandList.Select(c => c.CompanyId).Distinct().ToList();
            var activeCompanyIds = await _companyRepo.Query()
                .Where(x => companyIds.Contains(x.Id) && x.IsActive)
                .Select(x => x.Id)
                .ToListAsync(ct);

            var missingCompanyIds = companyIds.Except(activeCompanyIds).ToList();
            if (missingCompanyIds.Any())
                throw new KeyNotFoundException(
                    $"Company Id(s) not found or inactive: {string.Join(", ", missingCompanyIds)}");

            // --- تحقق من التكرار في IMO داخل الـ batch ---
            var imoSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var c in commandList.Where(c => !string.IsNullOrWhiteSpace(c.ImoNumber)))
            {
                if (!imoSet.Add(c.ImoNumber!))
                    throw new InvalidOperationException(
                        $"Duplicate IMO number found in the request: '{c.ImoNumber}'");
            }

            // --- تحقق من التكرار في DB بـ query واحدة ---
            var imoNumbers = imoSet.ToList();
            if (imoNumbers.Any())
            {
                var existingImos = await _repo.Query()
                    .Where(v => imoNumbers.Contains(v.IMONumber!))
                    .Select(v => v.IMONumber)
                    .ToListAsync(ct);

                if (existingImos.Any())
                    throw new InvalidOperationException(
                        $"IMO number(s) already exist: {string.Join(", ", existingImos)}");
            }

            // --- إنشاء دفعة واحدة ---
            var vessels = commandList
                .Select(c => Vessel.Create(
                    c.CompanyId,
                    c.VesselName,
                    c.ImoNumber,
                    c.GrossTonnage,
                    c.FlagCountryId,
                    c.Notes))
                .ToList();

            await _repo.AddRangeAsync(vessels, ct);
            await _repo.SaveChangesAsync(ct);

            return vessels
                .Select(vessel => new VesselResponse
                {
                    Id = vessel.Id,
                    CompanyId = vessel.CompanyID,
                    Name = vessel.VesselName,
                    IMONumber = vessel.IMONumber,
                    GrossTonnage = vessel.GrossTonnage,
                    FlagCountryId = vessel.FlagCountryID,
                    IsActive = vessel.IsActive
                })
                .ToList();
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
