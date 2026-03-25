using Microsoft.EntityFrameworkCore;
using Marilog.Domain.Entities;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Application.Interfaces.Services;
using Marilog.Application.DTOs;

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

        public async Task<CrewContractResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new CrewContractResponse
                {
                    ContractId = x.Id,
                    PersonId = x.PersonID,
                    PersonFullName = x.Person.FullName,
                    VesselId = x.VesselID,
                    VesselName = x.Vessel.VesselName,
                    RankId = x.RankID,
                    RankName = x.Rank.RankName,
                    RankDepartment = x.Rank.Department,
                    MonthlyWage = x.MonthlyWage,
                    SignOnDate = x.SignOnDate,
                    SignOffDate = x.SignOffDate ,
                    SignOnPort = x.SignOnPort,
                    SignOnPortName = x.SignOnPortNav!.PortName,
                    SignOffPort = x.SignOffPort,
                    SignOffPortName = x.SignOffPortNav!.PortName,
                    IsActive = x.IsActive
                })
                .FirstOrDefaultAsync(ct);
        }

        public async Task<IReadOnlyList<CrewContractResponse>> GetByPersonAsync(int personId,
            CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.PersonID == personId)
                .OrderByDescending(x => x.SignOnDate)
                .Select(x => new CrewContractResponse
                {
                    ContractId = x.Id,
                    PersonId = x.PersonID,
                    PersonFullName = x.Person.FullName,
                    VesselId = x.VesselID,
                    VesselName = x.Vessel.VesselName,
                    RankId = x.RankID,
                    RankName = x.Rank.RankName,
                    RankDepartment = x.Rank.Department,
                    MonthlyWage = x.MonthlyWage,
                    SignOnDate = x.SignOnDate,
                    SignOffDate = x.SignOffDate,
                    SignOnPort = x.SignOnPort,
                    SignOnPortName = x.SignOnPortNav!.PortName,
                    SignOffPort = x.SignOffPort,
                    SignOffPortName = x.SignOffPortNav!.PortName,
                    IsActive = x.IsActive
                })
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<CrewContractResponse>> GetByVesselAsync(int vesselId,
            CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.VesselID == vesselId)
                .OrderByDescending(x => x.SignOnDate)
                .Select(x => new CrewContractResponse
                {
                    ContractId = x.Id,
                    PersonId = x.PersonID,
                    PersonFullName = x.Person.FullName,
                    VesselId = x.VesselID,
                    VesselName = x.Vessel.VesselName,
                    RankId = x.RankID,
                    RankName = x.Rank.RankName,
                    RankDepartment = x.Rank.Department,
                    MonthlyWage = x.MonthlyWage,
                    SignOnDate = x.SignOnDate,
                    SignOffDate = x.SignOffDate,
                    SignOnPort = x.SignOnPort,
                    SignOnPortName = x.SignOnPortNav!.PortName,
                    SignOffPort = x.SignOffPort,
                    SignOffPortName = x.SignOffPortNav!.PortName,
                    IsActive = x.IsActive
                })
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<CrewContractResponse>> GetActiveByVesselAsync(int vesselId,
            CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.VesselID == vesselId && x.IsActive)
                .OrderBy(x => x.Rank.Department)
                .ThenBy(x => x.Person.FullName)
                .Select(x => new CrewContractResponse
                {
                    ContractId = x.Id,
                    PersonId = x.PersonID,
                    PersonFullName = x.Person.FullName,
                    VesselId = x.VesselID,
                    VesselName = x.Vessel.VesselName,
                    RankId = x.RankID,
                    RankName = x.Rank.RankName,
                    RankDepartment = x.Rank.Department,
                    MonthlyWage = x.MonthlyWage,
                    SignOnDate = x.SignOnDate,
                    SignOffDate = x.SignOffDate,
                    SignOnPort = x.SignOnPort,
                    SignOnPortName = x.SignOnPortNav!.PortName,
                    SignOffPort = x.SignOffPort,
                    SignOffPortName = x.SignOffPortNav!.PortName,
                    IsActive = x.IsActive
                })
                .ToListAsync(ct);
        }

        public async Task<CrewContractResponse?> GetActiveMasterAsync(int vesselId,
            CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.VesselID == vesselId && x.IsActive && x.Rank.RankCode == "MASTER")
                .Select(x => new CrewContractResponse
                {
                    ContractId = x.Id,
                    PersonId = x.PersonID,
                    PersonFullName = x.Person.FullName,
                    VesselId = x.VesselID,
                    VesselName = x.Vessel.VesselName,
                    RankId = x.RankID,
                    RankName = x.Rank.RankName,
                    RankDepartment = x.Rank.Department,
                    MonthlyWage = x.MonthlyWage,
                    SignOnDate = x.SignOnDate,
                    SignOffDate = x.SignOffDate,
                    SignOnPort = x.SignOnPort,
                    SignOnPortName = x.SignOnPortNav!.PortName,
                    SignOffPort = x.SignOffPort,
                    SignOffPortName = x.SignOffPortNav!.PortName,
                    IsActive = x.IsActive
                })
                .FirstOrDefaultAsync(ct);
        }

        public async Task<IReadOnlyList<CrewContractResponse>> GetActiveOnDateAsync(DateOnly date,
            CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.SignOnDate <= date && (!x.SignOffDate.HasValue || x.SignOffDate.Value >= date))
                .Select(x => new CrewContractResponse
                {
                    ContractId = x.Id,
                    PersonId = x.PersonID,
                    PersonFullName = x.Person.FullName,
                    VesselId = x.VesselID,
                    VesselName = x.Vessel.VesselName,
                    RankId = x.RankID,
                    RankName = x.Rank.RankName,
                    RankDepartment = x.Rank.Department,
                    MonthlyWage = x.MonthlyWage,
                    SignOnDate = x.SignOnDate,
                    SignOffDate = x.SignOffDate,
                    SignOnPort = x.SignOnPort,
                    SignOnPortName = x.SignOnPortNav!.PortName,
                    SignOffPort = x.SignOffPort,
                    SignOffPortName = x.SignOffPortNav!.PortName,
                    IsActive = x.IsActive
                })
                .ToListAsync(ct);
        }

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<CrewContractResponse> CreateAsync(int personId, int vesselId, int rankId,
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
            return new CrewContractResponse
            {
                ContractId = contract.Id,
                PersonId = contract.PersonID,
                PersonFullName = contract.Person.FullName,
                VesselId = contract.VesselID,
                VesselName = contract.Vessel.VesselName,
                RankId = contract.RankID,
                RankName = contract.Rank.RankName,
                RankDepartment = contract.Rank.Department,
                MonthlyWage = contract.MonthlyWage,
                SignOnDate = contract.SignOnDate,
                SignOffDate = contract.SignOffDate,
                SignOnPort = contract.SignOnPort,
                SignOnPortName = contract.SignOnPortNav!.PortName,
                SignOffPort = contract.SignOffPort,
                SignOffPortName = contract.SignOffPortNav!.PortName,
                IsActive = contract.IsActive
            };
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
                .AnyAsync(x => x.Id == personId && x.IsActive, ct);
            if (!exists)
                throw new KeyNotFoundException($"Person {personId} not found or inactive.");
        }

        private async Task EnsureVesselActiveAsync(int vesselId, CancellationToken ct)
        {
            var exists = await _vesselRepo.Query()
                .AnyAsync(x => x.Id == vesselId && x.IsActive, ct);
            if (!exists)
                throw new KeyNotFoundException($"Vessel {vesselId} not found or inactive.");
        }

        private async Task EnsureRankExistsAsync(int rankId, CancellationToken ct)
        {
            var exists = await _rankRepo.Query()
                .AnyAsync(x => x.Id == rankId && x.IsActive, ct);
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
