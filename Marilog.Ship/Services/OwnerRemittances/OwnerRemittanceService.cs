using Marilog.Domain.Entities.SystemEntities;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Kernel.Enums;
using Marilog.Ship.Domain.Entities;
using Marilog.Ship.Domain.Enums;
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

        public async Task CreateWagesPaidAshoreAsync(SaveWagesPaidAshoreRequest request, CancellationToken ct = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(request.Label);

            var items = request.Items.Where(x => x.Amount != 0).ToList();
            if (items.Count == 0)
                throw new InvalidOperationException("Enter the amount paid for at least one crew member.");
            if (items.GroupBy(x => x.CrewContractId).Any(g => g.Count() > 1))
                throw new InvalidOperationException("A crew member appears more than once.");

            foreach (var item in items)
                await EnsureShipCrewAsync(item.CrewContractId, ct);

            var remittances = items
                .Select(x => OwnerRemittance.Create(request.Date, OwnerRemittanceType.CrewPaidAshore, x.Amount,
                    request.CurrencyId, x.CrewContractId, null, request.Reference, request.Label))
                .ToList();

            await _repo.AddRangeAsync(remittances, ct);
            await _repo.SaveChangesAsync(ct);
        }

        // Wages paid ashore: a crew member of this ship whose wages are on the master's account.
        private async Task EnsureShipCrewAsync(int? crewContractId, CancellationToken ct)
        {
            if (crewContractId is null) return;

            var vesselId = await _setupRepo.Query().Select(x => x.VesselId).FirstOrDefaultAsync(ct);
            var paidBy = await _contractRepo.Query()
                .Where(x => x.Id == crewContractId && x.VesselID == vesselId)
                .Select(x => (WagesPaidBy?)x.WagesPaidBy)
                .FirstOrDefaultAsync(ct);

            if (paidBy is null)
                throw new InvalidOperationException("The crew contract does not belong to this vessel.");
            if (paidBy != WagesPaidBy.Master)
                throw new InvalidOperationException(
                    "Wages paid ashore are recorded only for crew whose wages are on the master's account.");
        }
    }
}