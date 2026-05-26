using Marilog.Kernel.Enums;

namespace Marilog.Presentation.PresentationDTOs
{
    public class FileUploadDto
    {
        public IFormFile File { get; set; } = null!;
        public EntityType EntityType { get; set; }
        public int? EntityId { get; set; }
    }
}
