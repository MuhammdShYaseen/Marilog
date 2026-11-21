using Marilog.Kernel.Enums;

namespace Marilog.Contracts.DTOs.Requests.DocumentTemplateDTOs
{
    // Create and update send the whole page.
    public class SaveDocumentTemplateRequest
    {
        public string Name { get; set; } = string.Empty;
        public TemplatePageSize PageSize { get; set; } = TemplatePageSize.A4;
        public bool IsLandscape { get; set; }
        public string FontFamily { get; set; } = "Noto Sans";
        public List<DocumentTemplateElementDto> Elements { get; set; } = [];
    }

    // Renders an unsaved design. With no DocumentId, sample values fill the fields.
    public class PreviewDocumentTemplateRequest
    {
        public SaveDocumentTemplateRequest Template { get; set; } = new();
        public int? DocumentId { get; set; }
    }
}