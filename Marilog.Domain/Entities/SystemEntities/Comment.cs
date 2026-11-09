using Marilog.Domain.Common;
using Marilog.Domain.Events;
using Marilog.Kernel.Enums;

namespace Marilog.Domain.Entities.SystemEntities
{
    public class Comment : Entity
    {
        public EntityType EntityType { get; private set; } = EntityType.NONE;
        public int? EntityId { get; private set; }

        public Guid UserId { get; private set; }
        public string UserName { get; private set; } = null!;
        public string Text { get; private set; } = null!;
        private readonly List<CommentRead> _reads = [];
        public IReadOnlyCollection<CommentRead> Reads => _reads.AsReadOnly();
        private Comment() { }

        public static Comment Create(string text, Guid userId, string userName, EntityType entityType, int? entityId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(text);

            if (userId == Guid.Empty)
                throw new ArgumentException("UserId is required.", nameof(userId));

            var comment = new Comment
            {
                Text = text.Trim(),
                UserId = userId,
                EntityType = entityType,
                EntityId = entityId,
                UserName = userName
            };
            //comment.AddDomainEvent(new CommentCreatedEvent(comment.UserId, comment.Text, comment.EntityType, comment.EntityId, comment.Guid));
            return comment;
        }

        public void UpdateText(string text)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(text);

            text = text.Trim();

            if (Text == text)
                return;

            Text = text;
            Touch();
        }


        public bool IsReadBy(Guid userId) => UserId == userId || _reads.Any(r => r.UserId == userId);

        public void MarkReadBy(Guid userId)
        {
            if (IsReadBy(userId)) return;
            _reads.Add(CommentRead.Create(userId));
        }
    }
}