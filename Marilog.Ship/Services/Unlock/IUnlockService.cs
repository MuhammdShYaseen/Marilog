using Marilog.Ship.Domain.Enums;

namespace Marilog.Ship.Services.Unlock
{
    public interface IUnlockService
    {
        /// <summary>The request still waiting for its unlock code, if any.</summary>
        Task<UnlockRequestResponse?> GetPendingAsync(UnlockTarget target, int? targetId, CancellationToken ct = default);

        /// <summary>New request code for one edit. Replaces any earlier request still waiting.</summary>
        Task<UnlockRequestResponse> RequestAsync(UnlockTarget target, int? targetId, string reason, CancellationToken ct = default);

        /// <summary>Checks the code from the office and reopens the target for one edit.</summary>
        Task ConfirmAsync(int requestId, string unlockCode, CancellationToken ct = default);
    }
}