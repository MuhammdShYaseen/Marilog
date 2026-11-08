using Marilog.Kernel.Enums;

namespace Marilog.Contracts.DTOs.Requests.CommentDTOs
{
    public sealed class CreateCommentRequest
    {
        public string Text { get; init; } = null!;
        public EntityType EntityType { get; init; }
        public int? EntityId { get; init; }
    }
}