using Marilog.Kernel.Enums;

namespace Marilog.Contracts.DTOs.Requests.NotificationDTOs
{
    public sealed class FollowRequest
    {
        public EntityType EntityType { get; init; }
        public int EntityId { get; init; }
        // The same label the Files and Comments dialogs get
        public string Title { get; init; } = string.Empty;
    }
}