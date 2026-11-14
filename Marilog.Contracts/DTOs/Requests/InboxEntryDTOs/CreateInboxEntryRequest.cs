using Marilog.Kernel.Enums;

namespace Marilog.Contracts.DTOs.Requests.InboxEntryDTOs
{
    public record CreateInboxEntryRequest
    (
        DateOnly ReceivedDate,
        InboxSource Source,
        List<int>? VesselIds,
        string Description,
        int? StartSequence,
        InboxDirection Direction = InboxDirection.In
    );
}