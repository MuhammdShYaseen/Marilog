using Marilog.Kernel.Enums;

namespace Marilog.Domain.Entities.UsersLog
{
    public class AuditLog
    {
        public long Id { get; private set; }
        public EntityType EntityType { get; private set; }
        public Guid EntityId { get; private set; }
        public AuditOperation Operation { get; private set; }
        public Guid? UserId { get; private set; }
        public string? UserName { get; private set; }
        public string? DataBefore { get; private set; }
        public string? DataAfter { get; private set; }
        public DateTime OccurredAt { get; private set; }

        private AuditLog() { }

        public static AuditLog Create(
            EntityType entityType,
            Guid entityId,
            AuditOperation operation,
            Guid? userId,
            string? userName,
            string? dataBefore,
            string? dataAfter)
        {
            return new AuditLog
            {
                EntityType = entityType,
                EntityId = entityId,
                Operation = operation,
                UserId = userId,
                UserName = userName,
                DataBefore = dataBefore,
                DataAfter = dataAfter,
                OccurredAt = DateTime.UtcNow
            };
        }
    }
}
