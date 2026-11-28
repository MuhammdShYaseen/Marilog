using Marilog.Domain.Entities.SystemEntities;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Ship.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Marilog.Ship.Services.OwnerRemittances
{
    public class OwnerRemittanceService : IOwnerRemittanceService
    {
        private readonly IRepository<OwnerRemittance> _repo;
        private readonly IRepository<CrewContract> _contractRepo;
        private readonly IRepository<ShipSetup> _setupRepo;

        public OwnerRemittanceService(
            IRepository<OwnerRemittance> repo,
            IRepository<CrewContract> contractRepo,
            IRepository<ShipSetup> setupRepo)
        {
            _repo = repo;
            _contractRepo = contractRepo;
            _setupRepo = setupRepo;
        }

        public async Task<IReadOnlyList<OwnerRemittanceResponse>> GetAsync(DateOnly from, DateOnly to,
            CancellationToken ct = default) =>
            await _repo.Query().AsNoTracking()
                .Where(x => x.Date >= from && x.Date <= to)
                .OrderByDescending(x => x.Date)
                .ThenByDescending(x => x.Id)
                .Select(x => new OwnerRemittanceResponse(
                    x.Id, x.Date, x.Type, x.Amount, x.CurrencyId, x.Currency.CurrencyCode,
                    x.CrewContractId,
                    x.CrewContract != null ? x.CrewContract.Person.FullName : null,
                    x.PortId,
                    x.Port != null ? x.Port.PortName : null,
                    x.Reference, x.Notes))
                .ToListAsync(ct);

        public async Task CreateAsync(SaveOwnerRemittanceRequest request, CancellationToken ct = default)
        {
            await EnsureShipCrewAsync(request.CrewContractId, ct);

            var remittance = OwnerRemittance.Create(request.Date, request.Type, request.Amount, request.CurrencyId,
                request.CrewContractId, request.PortId, request.Reference, request.Notes);

            await _repo.AddAsync(remittance, ct);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(int id, SaveOwnerRemittanceRequest request, CancellationToken ct = default)
        {
            await EnsureShipCrewAsync(request.CrewContractId, ct);

            var remittance = await _repo.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException($"Owner remittance {id} not found.");

            remittance.Update(request.Date, request.Type, request.Amount, request.CurrencyId,
                request.CrewContractId, request.PortId, request.Reference, request.Notes);

            await _repo.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var remittance = await _repo.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException($"Owner remittance {id} not found.");

            remittance.Delete();
            await _repo.SaveChangesAsync(ct);
        }

        // Wages paid ashore must be for a crew member of this ship.
        private async Task EnsureShipCrewAsync(int? crewContractId, CancellationToken ct)
        {
            if (crewContractId is null) return;

            var vesselId = await _setupRepo.Query().Select(x => x.VesselId).FirstOrDefaultAsync(ct);
            if (!await _contractRepo.Query().AnyAsync(x => x.Id == crewContractId && x.VesselID == vesselId, ct))
                throw new InvalidOperationException("The crew contract does not belong to this vessel.");
        }
    }
}