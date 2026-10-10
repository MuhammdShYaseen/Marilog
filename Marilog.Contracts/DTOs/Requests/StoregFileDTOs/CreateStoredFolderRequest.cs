using Marilog.Kernel.Enums;

namespace Marilog.Contracts.DTOs.Requests.StoregFileDTOs
{
    public class CreateStoredFolderRequest
    {
        public string Name { get; set; } = null!;
        public int? ParentFolderId { get; set; }
        public EntityType EntityType { get; set; }
        public int? EntityId { get; set; }
    }
}
