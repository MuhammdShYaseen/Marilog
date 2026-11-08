using Marilog.Kernel.Enums;

namespace Marilog.Contracts.DTOs.Responses
{
    public sealed class CommentResponse
    {
        public int Id { get; init; }
        public Guid UserId { get; init; }
        public string Text { get; init; } = null!;
        public EntityType EntityType { get; init; }
        public int? EntityId { get; init; }
        public DateTime CreatedAt { get; init; }
    }
}