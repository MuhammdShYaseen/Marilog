using Marilog.Domain.Common;
using Marilog.Kernel.Rules;
using Marilog.Ship.Domain.Enums;

namespace Marilog.Ship.Domain.Entities
{
    /// <summary>
    /// One request to edit something locked. The request code is sent to the office;
    /// the unlock code from the office works once, for this request only.
    /// Keeps who/when/why and the data before and after the edit.
    /// </summary>
    public class UnlockLog : Entity
    {
        public UnlockTarget Target { get; private set; }
        public int? TargetId { get; private set; }          // MGA Id when Target = Mga
        public string RequestCode { get; private set; } = null!;
        public string Reason { get; private set; } = null!;
        public DateTime? UnlockedAt { get; private set; }   // null = still waiting for the code
        public string? DataBefore { get; private set; }     // JSON
        public string? DataAfter { get; private set; }      // JSON, written when the edit is locked again

        private UnlockLog() { }

        public static UnlockLog Create(UnlockTarget target, int? targetId, string reason)
        {
            if (!Enum.IsDefined(target)) throw new ArgumentException("Invalid unlock target.");
            ArgumentException.ThrowIfNullOrWhiteSpace(reason);

            return new UnlockLog
            {
                Target = target,
                TargetId = targetId,
                RequestCode = ShipUnlockRules.NewRequestCode(),
                Reason = reason.Trim()
            };
        }

        public void Grant(string? dataBefore)
        {
            if (UnlockedAt is not null)
                throw new InvalidOperationException("This request code was already used. Ask for a new one.");

            UnlockedAt = DateTime.UtcNow;
            DataBefore = dataBefore;
            Touch();
        }

        public void Finish(string? dataAfter)
        {
            if (UnlockedAt is null)
                throw new InvalidOperationException("This request was never unlocked.");
            if (DataAfter is not null)
                throw new InvalidOperationException("This edit is already finished.");

            DataAfter = dataAfter;
            Touch();
        }
    }
}