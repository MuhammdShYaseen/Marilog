

using Marilog.Domain.Common;
using Marilog.Kernel.Enums;

namespace Marilog.Domain.Entities.SystemEntities
{
    public class InboxEntryLink : Entity
    {
        public int InboxEntryId { get; private set; }
        public EntityType EntityType { get; private set; }
        public int EntityId { get; private set; }

        private InboxEntryLink() { }

        public static InboxEntryLink Create(int inboxEntryId, EntityType entityType, int entityId)
            => new()
            {
                InboxEntryId = inboxEntryId,
                EntityType = entityType,
                EntityId = entityId
            };
    }
}
