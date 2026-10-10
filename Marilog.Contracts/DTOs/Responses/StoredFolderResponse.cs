

namespace Marilog.Contracts.DTOs.Responses
{
    public class StoredFolderResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int? ParentFolderId { get; set; }
    }
}
