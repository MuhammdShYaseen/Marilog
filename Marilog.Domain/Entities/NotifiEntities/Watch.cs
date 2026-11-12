using Marilog.Kernel.Enums;

namespace Marilog.Domain.Entities.NotifiEntities
{
    public class Watch
    {
        public int Id { get; private set; }
        public Guid UserId { get; private set; }
        public EntityType EntityType { get; private set; }
        public int EntityId { get; private set; }
        public Guid EntityGuid { get; private set; }
        public string Title { get; private set; } = "";       // max 300
        public DateTime CreatedAt { get; private set; }

        private Watch() { }



        public static Watch Create(Guid userId, EntityType entityType, int entityId, Guid entityGuid, string title) => new()
        {
            UserId = userId,
            EntityType = entityType,
            EntityId = entityId,
            EntityGuid = entityGuid,
            Title = title.Length > 300 ? title[..300] : title,
            CreatedAt = DateTime.UtcNow
        };
    }
}
