using Marilog.Kernel.Enums;

namespace Marilog.Presentation.PresentationDTOs
{
    public class FileUploadDto
    {
        public List<IFormFile> Files { get; set; } = [];
        public EntityType EntityType { get; set; }
        public int? EntityId { get; set; }
    }
}
