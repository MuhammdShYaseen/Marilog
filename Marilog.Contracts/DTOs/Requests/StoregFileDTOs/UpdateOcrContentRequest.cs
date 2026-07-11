

namespace Marilog.Contracts.DTOs.Requests.StoregFileDTOs
{
    public sealed class UpdateOcrContentRequest
    {
        public string Content { get; init; } = null!;
        public string? ThumbnailPath { get; init; }
    }
}
