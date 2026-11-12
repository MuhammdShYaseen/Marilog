using Marilog.Domain.Common;
using Marilog.Domain.Entities.NotifiEntities;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Infrastructure.DataAccess.ContextDb;
using Marilog.Infrastructure.Helpers.UserIdentity;
using Marilog.Kernel.Enums;

namespace Marilog.Infrastructure.Repositories
{
    public sealed class NotificationRepository : INotificationRepository
    {
        private readonly MarilogContext _db;

        public NotificationRepository(MarilogContext db) => _db = db;

        public IQueryable<Notification> Notifications() => _db.Set<Notification>();
        public IQueryable<Watch> Watches() => _db.Set<Watch>();

        public void Add(Notification notification) => _db.Set<Notification>().Add(notification);
        public void Add(Watch watch) => _db.Set<Watch>().Add(watch);
        public void Remove(Watch watch) => _db.Set<Watch>().Remove(watch);

        public async Task<Guid?> GetEntityGuidAsync(EntityType type, int id, CancellationToken ct = default)
        {
            // EntityType values are the class names: same rule the audit uses (AuditEntityTypes)
            var clrType = _db.Model.GetEntityTypes()
                .Select(t => t.ClrType)
                .FirstOrDefault(t => typeof(Entity).IsAssignableFrom(t) && AuditEntityTypes.Resolve(t) == type);

            if (clrType is null) return null;

            var record = await _db.FindAsync(clrType, new object[] { id }, ct) as Entity;
            return record?.Guid;
        }

        public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
    }
}