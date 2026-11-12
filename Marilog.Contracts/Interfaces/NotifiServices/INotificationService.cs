using Marilog.Contracts.DTOs.Requests.NotificationDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Kernel.Enums;

namespace Marilog.Contracts.Interfaces.NotifiServices
{
    public interface INotificationService
    {
        // ── Follow ────────────────────────────────────────────────
        Task<bool> IsFollowingAsync(EntityType type, int id, CancellationToken ct = default);

        /// <summary>False when the record doesn't exist.</summary>
        Task<bool> FollowAsync(FollowRequest request, CancellationToken ct = default);

        Task UnfollowAsync(EntityType type, int id, CancellationToken ct = default);

        // ── Notifications ─────────────────────────────────────────
        // readableTypes: set by the server from the user's permissions; the client leaves it null.
        Task<IReadOnlyList<NotificationResponse>> GetMineAsync(IReadOnlyCollection<EntityType>? readableTypes = null, CancellationToken ct = default);
        Task<int> GetUnreadCountAsync(IReadOnlyCollection<EntityType>? readableTypes = null, CancellationToken ct = default);
        Task MarkReadAsync(long id, CancellationToken ct = default);
        Task MarkAllReadAsync(CancellationToken ct = default);
    }
}