using Marilog.Contracts.Models;

namespace Marilog.Contracts.DTOs.LogDTOs
{
    public sealed record LogReadResult(
    IReadOnlyList<LogEntry> Items,
    int TotalCount,
    int Page,
    int PageSize,
    LogStats Stats
);
}
