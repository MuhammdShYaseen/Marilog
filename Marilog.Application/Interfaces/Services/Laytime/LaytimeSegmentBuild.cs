

using Marilog.Domain.Entities.LaytimeEntities;

namespace Marilog.Application.Interfaces.Services.Laytime
{
    /// <summary>
    /// Output of LaytimeEngine.BuildSegments.
    /// LaytimeExpiredAt: when laytime ran out (null if it never did).
    /// AllTimeSaved: calendar time from completion to the hypothetical expiry (LT-07); null if laytime expired.
    /// </summary>
    public sealed record LaytimeSegmentBuild(
    decimal AllowedDays,
    IReadOnlyList<LaytimeSegment> Segments,
    DateTime? LaytimeExpiredAt,
    TimeSpan? AllTimeSaved);
}
