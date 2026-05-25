using Marilog.Kernel.Enums;

namespace Marilog.Contracts.DTOs.Requests.StoregFileDTOs
{
    public class UploadFileRequest
    {
        public Stream FileStream { get; init; } = null!;
        public string FileName { get; init; } = null!;
        public string ContentType { get; init; } = null!;
        public long Size { get; init; }
        public EntityType EntityType { get; init; }
        public int? EntityId { get; init; }
    }
}
