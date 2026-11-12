using Marilog.Kernel.Enums;
namespace Marilog.Domain.Entities.NotifiEntities
{
    public class Notification
    {
        public long Id { get; private set; }
        public Guid UserId { get; private set; }
        public string Text { get; private set; }        // max 500
        public EntityType EntityType { get; private set; }
        public int EntityId { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? ReadAt { get; private set; }
        public string DedupKey { get; private set; }    // max 200, e.g. "audit:{logId}"

        private Notification() { }
    }
}
