using Marilog.Kernel.Enums;

namespace Marilog.Contracts.DTOs.Responses
{
    public sealed class UnreadCommentCount
    {
        public EntityType EntityType { get; init; }
        public int EntityId { get; init; }
        public int Count { get; init; }
    }
}