using Marilog.Domain.Common;
using Marilog.Kernel.Enums;

namespace Marilog.Domain.Events
{
    public class CommentCreatedEvent : IDomainEvent
    {
        public Guid UserId { get; }
        public Guid CommentId { get; }
        public string Comment { get; }
        public EntityType EntityType { get; }
        public int? EntityId { get; }

        public CommentCreatedEvent(Guid userId, string comment, EntityType entityType, int? entityId, Guid commentId)
        {
            UserId = userId;
            Comment = comment;
            EntityType = entityType;
            EntityId = entityId;
            CommentId = commentId;
        }
    }
}