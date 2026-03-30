using Marilog.Domain.ReadModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Application.DTOs.LogDTOs
{
    public sealed record LogReadResult(
    IReadOnlyList<LogEntry> Items,
    int TotalCount,
    int Page,
    int PageSize,
    LogStats Stats
);
}
