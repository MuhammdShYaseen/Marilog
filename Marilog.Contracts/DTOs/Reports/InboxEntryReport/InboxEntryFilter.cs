

using Marilog.Kernel.Enums;

namespace Marilog.Contracts.DTOs.Reports.InboxEntryReport
{
    public sealed class InboxEntryFilter
    {

    private const int MaxPageSize = 100;
    private int _page = 1;
    private int _pageSize = 20;

    public int Page
    {
        get => _page;
        init => _page = value <= 0 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = value <= 0
            ? 20
            : Math.Min(value, MaxPageSize);
    }
public string? SearchTerm { get; init; }
        public int? Year { get; init; }
        public DateOnly? From { get; init; }
        public DateOnly? To { get; init; }
        public InboxSource? Source { get; init; }
        public int? VesselId { get; init; }
        public bool? IsProcessed { get; init; }
        public bool? IsCancelled { get; init; }
        public EntityType? LinkedEntityType { get; init; }
        public int? LinkedEntityId { get; init; }
    }

    public record InboxEntryResponse
    (
        int Id,
        int Year,
        int Sequence,
        string Number,
        DateOnly ReceivedDate,
        InboxSource Source,
        List<int> VesselIds,
        string Description,
        bool IsProcessed,
        bool IsCancelled
    )
    {
        public List<InboxEntryLinkResponse> Links { get; init; } = [];
    }

    public record InboxEntryLinkResponse(int Id, EntityType EntityType, int EntityId)
    {
        public string? Label { get; init; }
    }

    public record LinkCandidateResponse(int Id, string Label);
}
