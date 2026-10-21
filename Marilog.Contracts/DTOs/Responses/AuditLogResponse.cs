using Marilog.Kernel.Enums;

namespace Marilog.Contracts.DTOs.Responses
{

    // List row: no JSON payload, keeps pages light
    public sealed class AuditLogListItem
    {
        public long Id { get; init; }
        public EntityType EntityType { get; init; }
        public Guid EntityId { get; init; }
        public AuditOperation Operation { get; init; }
        public Guid? UserId { get; init; }
        public string? UserName { get; init; }
        public DateTime OccurredAt { get; init; }
    }

    public sealed class AuditLogResponse
    {
        public long Id { get; init; }
        public EntityType EntityType { get; init; }
        public Guid EntityId { get; init; }
        public AuditOperation Operation { get; init; }
        public Guid? UserId { get; init; }
        public string? UserName { get; init; }
        public string? DataBefore { get; init; }
        public string? DataAfter { get; init; }
        public DateTime OccurredAt { get; init; }
    }

    public sealed class PurgeAuditLogsResponse
    {
        public int DeletedCount { get; init; }
        public DateTime Cutoff { get; init; }
        public int RetentionDays { get; init; }
    } 
}
