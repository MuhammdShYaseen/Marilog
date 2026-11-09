namespace Marilog.Domain.Entities.SystemEntities
{
    public class CommentRead
    {
        public Guid UserId { get; private set; }
        public DateTime ReadAt { get; private set; }

        private CommentRead() { }

        internal static CommentRead Create(Guid userId) => new()
        {
            UserId = userId,
            ReadAt = DateTime.UtcNow
        };
    }
}