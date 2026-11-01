

using Marilog.Kernel.Enums;

namespace Marilog.Contracts.DTOs.Requests.InboxEntryDTOs
{
    public record CreateInboxEntryItem(
    DateOnly ReceivedDate,
    InboxSource Source,
    List<int>? VesselIds,
    string Description,
    int? Sequence = null);
}
