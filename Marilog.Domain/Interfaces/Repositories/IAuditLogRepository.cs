using Marilog.Domain.Entities.UsersLog;

namespace Marilog.Domain.Interfaces.Repositories
{
    /// <summary>
    /// AuditLog is not an Entity, so it has its own repository.
    /// </summary>
    public interface IAuditLogRepository
    {
        IQueryable<AuditLog> Query();

        /// <summary>
        /// Deletes logs older than <paramref name="cutoff"/> and saves the purge record, in one transaction.
        /// Records about audit management itself (EntityType.AuditLog) are never deleted.
        /// Returns the deleted count.
        /// </summary>
        Task<int> PurgeAsync(DateTime cutoff, Func<int, AuditLog> buildPurgeRecord, CancellationToken ct = default);

        Task AddAsync(AuditLog log, CancellationToken ct = default);
    }
}
