using Marilog.Domain.Interfaces.Repositories;
using Marilog.Kernel.Rules;
using Marilog.Ship.Domain.Entities;
using Marilog.Ship.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Marilog.Ship.Services.Unlock
{
    public class UnlockService : IUnlockService
    {
        private readonly IRepository<UnlockLog> _logRepo;
        private readonly IRepository<ShipSetup> _setupRepo;
        private readonly IRepository<Mga> _mgaRepo;

        public UnlockService(
            IRepository<UnlockLog> logRepo,
            IRepository<ShipSetup> setupRepo,
            IRepository<Mga> mgaRepo)
        {
            _logRepo = logRepo;
            _setupRepo = setupRepo;
            _mgaRepo = mgaRepo;
        }

        public async Task<UnlockRequestResponse?> GetPendingAsync(UnlockTarget target, int? targetId,
            CancellationToken ct = default) =>
            await _logRepo.Query().AsNoTracking()
                .Where(x => x.Target == target && x.TargetId == targetId && x.UnlockedAt == null)
                .OrderByDescending(x => x.Id)
                .Select(x => new UnlockRequestResponse(x.Id, x.RequestCode, x.Reason, x.CreatedAt))
                .FirstOrDefaultAsync(ct);

        public async Task<UnlockRequestResponse> RequestAsync(UnlockTarget target, int? targetId, string reason,
            CancellationToken ct = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(reason);

            var setup = await _setupRepo.Query().FirstOrDefaultAsync(ct);
            if (setup is not { IsCompleted: true })
                throw new InvalidOperationException("The setup is not locked.");
            if (setup.ShipKeyEncrypted is null)
                throw new InvalidOperationException(
                    "No ship key is saved on this ship, so it cannot be unlocked. Reinstall with a ship key from the office.");

            switch (target)
            {
                case UnlockTarget.InitialSetup:
                    // The opening balances are the start of every MGA after them.
                    if (await _mgaRepo.Query().AnyAsync(x => !x.IsOpening, ct))
                        throw new InvalidOperationException(
                            "MGAs were closed after the setup. Reopen them first, starting from the last one.");
                    break;

                case UnlockTarget.Mga:
                    await EnsureLastClosedAsync(targetId, ct);
                    break;

                default:
                    throw new NotSupportedException($"Unlock target {target} is not supported.");
            }

            // Only one request waits at a time: an older code can never be used.
            var waiting = await _logRepo.Query()
                .Where(x => x.Target == target && x.TargetId == targetId && x.UnlockedAt == null)
                .ToListAsync(ct);
            _logRepo.RemoveRange(waiting);

            var log = UnlockLog.Create(target, targetId, reason);
            await _logRepo.AddAsync(log, ct);
            await _logRepo.SaveChangesAsync(ct);

            return new UnlockRequestResponse(log.Id, log.RequestCode, log.Reason, log.CreatedAt);
        }

        public async Task ConfirmAsync(int requestId, string unlockCode, CancellationToken ct = default)
        {
            var log = await _logRepo.GetByIdAsync(requestId, ct)
                ?? throw new KeyNotFoundException("Unlock request not found. Ask for a new one.");
            if (log.UnlockedAt is not null)
                throw new InvalidOperationException("This request code was already used. Ask for a new one.");

            var setup = await _setupRepo.Query().FirstOrDefaultAsync(ct)
                ?? throw new InvalidOperationException("The setup has not been started.");
            if (setup.ShipKeyEncrypted is null)
                throw new InvalidOperationException("No ship key is saved on this ship.");

            var shipKey = ShipKeyProtector.Unprotect(setup.ShipKeyEncrypted);
            if (!ShipUnlockRules.IsUnlockCodeCorrect(shipKey, log.RequestCode, unlockCode))
                throw new InvalidOperationException("The unlock code is not correct.");

            switch (log.Target)
            {
                case UnlockTarget.InitialSetup:
                    var opening = await _mgaRepo.Query()
                        .Include(x => x.CrewLines)
                        .Include(x => x.CashLines)
                        .FirstAsync(x => x.IsOpening, ct);

                    log.Grant(OpeningSnapshot.ToJson(opening));
                    setup.Reopen();
                    opening.ReopenOpening();
                    break;

                case UnlockTarget.Mga:
                    // The MGA is removed: its period is computed live again and closed again
                    // after the corrections. Its figures stay in this log.
                    var mga = await EnsureLastClosedAsync(log.TargetId, ct);

                    log.Grant(MgaSnapshot.ToJson(mga));
                    _mgaRepo.HardDelete(mga);
                    break;

                default:
                    throw new NotSupportedException($"Unlock target {log.Target} is not supported.");
            }

            await _logRepo.SaveChangesAsync(ct);
        }

        // Only the last closed MGA can be reopened, so the chain of balances stays whole.
        private async Task<Mga> EnsureLastClosedAsync(int? mgaId, CancellationToken ct)
        {
            var last = await _mgaRepo.Query()
                .Include(x => x.CrewLines)
                .Include(x => x.CashLines)
                .Where(x => x.Status == MgaStatus.Closed)
                .OrderByDescending(x => x.Number)
                .FirstOrDefaultAsync(ct);

            if (last is null || last.IsOpening || last.Id != mgaId)
                throw new InvalidOperationException("Only the last closed MGA can be reopened.");

            return last;
        }
    }
}