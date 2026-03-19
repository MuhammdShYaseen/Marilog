using Microsoft.EntityFrameworkCore;
using Marilog.Domain.Entities;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Domain.Interfaces.Services;

namespace Marilog.Application.Services
{
    public class CrewContractService : ICrewContractService
    {
        private readonly IRepository<CrewContract> _repo;
        private readonly IRepository<Vessel>       _vesselRepo;
        private readonly IRepository<Person>       _personRepo;
        private readonly IRepository<Rank>         _rankRepo;

        public CrewContractService(
            IRepository<CrewContract> repo,
            IRepository<Vessel>       vesselRepo,
            IRepository<Person>       personRepo,
            IRepository<Rank>         rankRepo)
        {
            _repo       = repo;
            _vesselRepo = vesselRepo;
            _personRepo = personRepo;
            _rankRepo   = rankRepo;
        }

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<CrewContract?> GetByIdAsync(int id, CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .Include(x => x.Person)
                          .Include(x => x.Vessel)
                          .Include(x => x.Rank)
                          .Include(x => x.SignOnPortNav)
                          .Include(x => x.SignOffPortNav)
                          .FirstOrDefaultAsync(x => x.ContractID == id, ct);

        public async Task<IReadOnlyList<CrewContract>> GetByPersonAsync(int personId,
            CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .Where(x => x.PersonID == personId)
                          .Include(x => x.Vessel)
                          .Include(x => x.Rank)
                          .OrderByDescending(x => x.SignOnDate)
                          .ToListAsync(ct);

        public async Task<IReadOnlyList<CrewContract>> GetByVesselAsync(int vesselId,
            CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .Where(x => x.VesselID == vesselId)
                          .Include(x => x.Person)
                          .Include(x => x.Rank)
                          .OrderByDescending(x => x.SignOnDate)
                          .ToListAsync(ct);

        public async Task<IReadOnlyList<CrewContract>> GetActiveByVesselAsync(int vesselId,
            CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .Where(x => x.VesselID == vesselId && x.IsActive)
                          .Include(x => x.Person)
                          .Include(x => x.Rank)
                          .OrderBy(x => x.Rank.Department)
                          .ThenBy(x => x.Person.FullName)
                          .ToListAsync(ct);

        public async Task<CrewContract?> GetActiveMasterAsync(int vesselId,
            CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .Where(x => x.VesselID  == vesselId  &&
                                      x.IsActive                &&
                                      x.Rank.RankCode == "MASTER")
                          .Include(x => x.Person)
                          .Include(x => x.Rank)
                          .FirstOrDefaultAsync(ct);

        public async Task<IReadOnlyList<CrewContract>> GetActiveOnDateAsync(DateOnly date,
            CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .Where(x => x.SignOnDate  <= date &&
                                      (!x.SignOffDate.HasValue || x.SignOffDate.Value >= date))
                          .Include(x => x.Person)
                          .Include(x => x.Vessel)
                          .Include(x => x.Rank)
                          .ToListAsync(ct);

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<CrewContract> CreateAsync(int personId, int vesselId, int rankId,
            decimal monthlyWage, DateOnly signOnDate, int? signOnPort = null,
            string? notes = null, CancellationToken ct = default)
        {
            await EnsurePersonExistsAsync(personId, ct);
            await EnsureVesselActiveAsync(vesselId, ct);
            await EnsureRankExistsAsync(rankId, ct);
            await EnsureNoActiveContractAsync(personId, vesselId, signOnDate, ct);

            var contract = CrewContract.Create(personId, vesselId, rankId,
                                               monthlyWage, signOnDate, signOnPort, notes);
            await _repo.AddAsync(contract, ct);
            await _repo.SaveChangesAsync(ct);
            return contract;
        }

        public async Task UpdateAsync(int id, decimal monthlyWage,
            string? notes = null, CancellationToken ct = default)
        {
            var contract = await GetOrThrowAsync(id, ct);
            contract.Update(monthlyWage, notes);
            _repo.Update(contract);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task SignOffAsync(int id, DateOnly signOffDate,
            int? signOffPort = null, CancellationToken ct = default)
        {
            var contract = await GetOrThrowAsync(id, ct);
            contract.SignOff(signOffDate, signOffPort);
            _repo.Update(contract);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var contract = await GetOrThrowAsync(id, ct);
            if (!contract.SignOffDate.HasValue)
                throw new InvalidOperationException(
                    "Cannot delete an active contract. Sign off the crew member first.");
            _repo.HardDelete(contract);
            await _repo.SaveChangesAsync(ct);
        }

        // ── Private ───────────────────────────────────────────────────────────────

        private async Task<CrewContract> GetOrThrowAsync(int id, CancellationToken ct)
            => await _repo.GetByIdAsync(id, ct)
               ?? throw new KeyNotFoundException($"CrewContract {id} not found.");

        private async Task EnsurePersonExistsAsync(int personId, CancellationToken ct)
        {
            var exists = await _personRepo.Query()
                .AnyAsync(x => x.PersonID == personId && x.IsActive, ct);
            if (!exists)
                throw new KeyNotFoundException($"Person {personId} not found or inactive.");
        }

        private async Task EnsureVesselActiveAsync(int vesselId, CancellationToken ct)
        {
            var exists = await _vesselRepo.Query()
                .AnyAsync(x => x.VesselID == vesselId && x.IsActive, ct);
            if (!exists)
                throw new KeyNotFoundException($"Vessel {vesselId} not found or inactive.");
        }

        private async Task EnsureRankExistsAsync(int rankId, CancellationToken ct)
        {
            var exists = await _rankRepo.Query()
                .AnyAsync(x => x.RankID == rankId && x.IsActive, ct);
            if (!exists)
                throw new KeyNotFoundException($"Rank {rankId} not found or inactive.");
        }

        private async Task EnsureNoActiveContractAsync(int personId, int vesselId,
            DateOnly signOnDate, CancellationToken ct)
        {
            var conflict = await _repo.Query()
                .AnyAsync(x => x.PersonID   == personId &&
                               x.VesselID   == vesselId &&
                               x.IsActive               &&
                               x.SignOnDate <= signOnDate &&
                               (!x.SignOffDate.HasValue || x.SignOffDate.Value >= signOnDate), ct);
            if (conflict)
                throw new InvalidOperationException(
                    "This crew member already has an active contract on this vessel.");
        }
    }
}
