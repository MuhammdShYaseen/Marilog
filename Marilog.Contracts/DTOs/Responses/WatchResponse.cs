using Marilog.Kernel.Enums;

namespace Marilog.Contracts.DTOs.Responses
{
    public sealed class WatchResponse
    {
        public EntityType EntityType { get; init; }
        public int EntityId { get; init; }
        public string Title { get; init; } = string.Empty;
        public DateTime FollowedAt { get; init; }
        public DateTime? LastActivityAt { get; init; }
        // Settable: the page clears it once the record's history is opened
        public int UnreadCount { get; set; }
    }
}