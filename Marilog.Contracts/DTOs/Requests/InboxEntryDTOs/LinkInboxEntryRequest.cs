using Marilog.Kernel.Enums;

namespace Marilog.Contracts.DTOs.Requests.InboxEntryDTOs
{
    public record LinkInboxEntryRequest(EntityType EntityType, int EntityId);
}
