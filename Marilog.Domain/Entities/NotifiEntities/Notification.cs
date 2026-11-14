using Marilog.Kernel.Enums;
namespace Marilog.Domain.Entities.NotifiEntities
{
    public class Notification
    {
        public long Id { get; private set; }
        public Guid UserId { get; private set; }
        public string Text { get; private set; } = "";        // max 500
        public EntityType EntityType { get; private set; }
        public int EntityId { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? ReadAt { get; private set; }
        public DateTime? EmailedAt { get; private set; }    // set once it went out in the daily email
        public string DedupKey { get; private set; } = "";    // max 200, e.g. "audit:{logId}"

        private Notification() { }


        public static Notification Create(Guid userId, string text, EntityType entityType, int entityId, string dedupKey) => new()
        {
            UserId = userId,
            Text = text.Length > 500 ? text[..500] : text,
            EntityType = entityType,
            EntityId = entityId,
            DedupKey = dedupKey,
            CreatedAt = DateTime.UtcNow
        };

        public void MarkEmailed() => EmailedAt ??= DateTime.UtcNow;
        public void MarkRead() => ReadAt ??= DateTime.UtcNow;
    }
}
