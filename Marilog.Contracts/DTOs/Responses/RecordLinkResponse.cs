using Marilog.Kernel.Enums;

namespace Marilog.Contracts.DTOs.Responses
{
    public sealed class RecordLinkResponse
    {
        // The record the page shows: the parent when a child record was asked for.
        public EntityType EntityType { get; init; }
        public int EntityId { get; init; }
        public string Url { get; init; } = string.Empty;
    }
}