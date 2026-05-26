using Marilog.Kernel.Enums;

namespace Marilog.Contracts.DTOs.Requests.StoregFileDTOs
{
    public class UpdateEntityLinkRequest
    {
        public EntityType EntityType { get; init; }
        public int? EntityId { get; init; }
    }
}
