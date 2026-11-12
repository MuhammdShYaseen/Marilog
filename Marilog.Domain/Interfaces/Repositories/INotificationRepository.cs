using Marilog.Domain.Entities.NotifiEntities;
using Marilog.Kernel.Enums;

namespace Marilog.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Notifications and watches are plain tables, not Entity: not audited, no soft delete.
    /// </summary>
    public interface INotificationRepository
    {
        IQueryable<Notification> Notifications();
        IQueryable<Watch> Watches();

        void Add(Notification notification);
        void Add(Watch watch);
        void Remove(Watch watch);

        /// <summary>The Guid of any record by its type and Id; null when it doesn't exist.</summary>
        Task<Guid?> GetEntityGuidAsync(EntityType type, int id, CancellationToken ct = default);

        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}