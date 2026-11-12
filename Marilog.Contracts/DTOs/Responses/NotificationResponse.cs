using Marilog.Kernel.Enums;

namespace Marilog.Contracts.DTOs.Responses
{
    public sealed class NotificationResponse
    {
        public long Id { get; init; }
        public string Text { get; init; } = string.Empty;
        public EntityType EntityType { get; init; }
        public int EntityId { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? ReadAt { get; init; }
    }
}