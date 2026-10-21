using Marilog.Domain.Entities.UsersLog;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Infrastructure.DataAccess.ContextDb;
using Marilog.Kernel.Enums;
using Microsoft.EntityFrameworkCore;

namespace Marilog.Infrastructure.Repositories
{
    public sealed class AuditLogRepository : IAuditLogRepository
    {
        private readonly MarilogContext _db;

        public AuditLogRepository(MarilogContext db)
        {
            _db = db;
        }

        public IQueryable<AuditLog> Query() => _db.Set<AuditLog>();

        public async Task<int> PurgeAsync(DateTime cutoff, Func<int, AuditLog> buildPurgeRecord, CancellationToken ct = default)
        {
            // EnableRetryOnFailure forbids a plain user transaction: it must run inside the strategy
            var strategy = _db.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                // A retry re-runs this block; drop anything a failed attempt left tracked
                _db.ChangeTracker.Clear();

                await using var tx = await _db.Database.BeginTransactionAsync(ct);

                // Purge records are never deleted
                // Records about audit management itself (purges, settings changes) are never deleted
                var count = await _db.Set<AuditLog>()
                    .Where(l => l.OccurredAt < cutoff && l.EntityType != EntityType.AuditLog)
                                    .ExecuteDeleteAsync(ct);

                _db.Set<AuditLog>().Add(buildPurgeRecord(count));

                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                return count;
            });
        }

        public async Task AddAsync(AuditLog log, CancellationToken ct = default)
        {
            _db.Set<AuditLog>().Add(log);
            await _db.SaveChangesAsync(ct);
        }
    }
}