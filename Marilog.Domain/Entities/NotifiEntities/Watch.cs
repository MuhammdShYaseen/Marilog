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
        public string Title { get; private set; }       // max 300
        public DateTime CreatedAt { get; private set; }

        private Watch() { }
    }
}
