
using Marilog.Kernel.Enums;

namespace Marilog.Contracts.DTOs.Requests.EmailDTOs
{
    public class UpsertEmailEntityRequest
    {
        public EntityType EntityType { get; set; }
        public int EntityId { get; set; }
    }
}
