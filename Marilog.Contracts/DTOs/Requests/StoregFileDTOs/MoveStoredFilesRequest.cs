

namespace Marilog.Contracts.DTOs.Requests.StoregFileDTOs
{
    public class MoveStoredFilesRequest
    {
        public List<int> FileIds { get; set; } = [];
        public int? TargetFolderId { get; set; }
    }
}
