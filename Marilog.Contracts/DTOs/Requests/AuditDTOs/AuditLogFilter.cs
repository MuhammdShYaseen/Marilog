using Marilog.Kernel.Enums;

namespace Marilog.Contracts.DTOs.Requests.AuditDTOs
{
    public sealed class AuditLogFilter
    {
        public EntityType? EntityType { get; init; }
        public Guid? EntityId { get; init; }
        public Guid? UserId { get; init; }
        public AuditOperation? Operation { get; init; }

        // Inclusive UTC dates
        public DateOnly? From { get; init; }
        public DateOnly? To { get; init; }
    }
}
