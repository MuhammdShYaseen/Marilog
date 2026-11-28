using Marilog.Domain.Entities.SystemEntities;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Ship.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Marilog.Ship.Services.CrewAccount
{
    public class CrewAccountService : ICrewAccountService
    {
        private readonly IRepository<CrewWithdrawal> _withdrawalRepo;
        private readonly IRepository<CrewExtra> _extraRepo;
        private readonly IRepository<CrewContract> _contractRepo;
        private readonly IRepository<ShipSetup> _setupRepo;

        public CrewAccountService(
            IRepository<CrewWithdrawal> withdrawalRepo,
            IRepository<CrewExtra> extraRepo,
            IRepository<CrewContract> contractRepo,
            IRepository<ShipSetup> setupRepo)
        {
            _withdrawalRepo = withdrawalRepo;
            _extraRepo = extraRepo;
            _contractRepo = contractRepo;
            _setupRepo = setupRepo;
        }

        // ── Withdrawals ──────────────────────────────────────────────────────────

        public async Task<IReadOnlyList<CrewWithdrawalResponse>> GetWithdrawalsAsync(DateOnly from, DateOnly to,
            CancellationToken ct = default)
        {
            // Withdrawals keep the time: the whole of the last day is included.
            var start = from.ToDateTime(TimeOnly.MinValue);
            var end = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

            return await _withdrawalRepo.Query().AsNoTracking()
                .Where(x => x.Date >= start && x.Date < end)
                .OrderByDescending(x => x.Date)
                .ThenByDescending(x => x.Id)
                .Select(x => new CrewWithdrawalResponse(
                    x.Id, x.Date, x.CrewContractId, x.CrewContract.Person.FullName, x.CrewContract.Rank.RankName,
                    x.Type, x.DeductionTypeId,
                    x.DeductionType != null ? x.DeductionType.Name : null,
                    x.Amount, x.CurrencyId, x.Currency.CurrencyCode, x.Reason))
                .ToListAsync(ct);
        }

        public async Task CreateWithdrawalAsync(SaveCrewWithdrawalRequest request, CancellationToken ct = default)
        {
            await EnsureShipCrewAsync(request.CrewContractId, ct);

            var withdrawal = CrewWithdrawal.Create(request.Date, request.CrewContractId, request.Type,
                request.Amount, request.CurrencyId, request.DeductionTypeId, request.Reason);

            await _withdrawalRepo.AddAsync(withdrawal, ct);
            await _withdrawalRepo.SaveChangesAsync(ct);
        }

        public async Task UpdateWithdrawalAsync(int id, SaveCrewWithdrawalRequest request, CancellationToken ct = default)
        {
            await EnsureShipCrewAsync(request.CrewContractId, ct);

            var withdrawal = await _withdrawalRepo.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException($"Crew withdrawal {id} not found.");

            withdrawal.Update(request.Date, request.CrewContractId, request.Type,
                request.Amount, request.CurrencyId, request.DeductionTypeId, request.Reason);

            await _withdrawalRepo.SaveChangesAsync(ct);
        }

        public async Task DeleteWithdrawalAsync(int id, CancellationToken ct = default)
        {
            var withdrawal = await _withdrawalRepo.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException($"Crew withdrawal {id} not found.");

            withdrawal.Delete();
            await _withdrawalRepo.SaveChangesAsync(ct);
        }

        // ── Extras ───────────────────────────────────────────────────────────────

        public async Task<IReadOnlyList<CrewExtraResponse>> GetExtrasAsync(DateOnly from, DateOnly to,
            CancellationToken ct = default) =>
            await _extraRepo.Query().AsNoTracking()
                .Where(x => x.Date >= from && x.Date <= to)
                .OrderByDescending(x => x.Date)
                .ThenByDescending(x => x.Id)
                .Select(x => new CrewExtraResponse(
                    x.Id, x.Date, x.CrewContractId, x.CrewContract.Person.FullName, x.CrewContract.Rank.RankName,
                    x.Type, x.Amount, x.CurrencyId, x.Currency.CurrencyCode, x.Reason))
                .ToListAsync(ct);

        public async Task CreateExtraAsync(SaveCrewExtraRequest request, CancellationToken ct = default)
        {
            await EnsureShipCrewAsync(request.CrewContractId, ct);

            var extra = CrewExtra.Create(request.Date, request.CrewContractId, request.Type,
                request.Amount, request.CurrencyId, request.Reason);

            await _extraRepo.AddAsync(extra, ct);
            await _extraRepo.SaveChangesAsync(ct);
        }

        public async Task UpdateExtraAsync(int id, SaveCrewExtraRequest request, CancellationToken ct = default)
        {
            await EnsureShipCrewAsync(request.CrewContractId, ct);

            var extra = await _extraRepo.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException($"Crew extra {id} not found.");

            extra.Update(request.Date, request.CrewContractId, request.Type,
                request.Amount, request.CurrencyId, request.Reason);

            await _extraRepo.SaveChangesAsync(ct);
        }

        public async Task DeleteExtraAsync(int id, CancellationToken ct = default)
        {
            var extra = await _extraRepo.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException($"Crew extra {id} not found.");

            extra.Delete();
            await _extraRepo.SaveChangesAsync(ct);
        }

        // ── Private ──────────────────────────────────────────────────────────────

        private async Task EnsureShipCrewAsync(int crewContractId, CancellationToken ct)
        {
            var vesselId = await _setupRepo.Query().Select(x => x.VesselId).FirstOrDefaultAsync(ct);
            if (!await _contractRepo.Query().AnyAsync(x => x.Id == crewContractId && x.VesselID == vesselId, ct))
                throw new InvalidOperationException("The crew contract does not belong to this vessel.");
        }
    }
}