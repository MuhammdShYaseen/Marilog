using Marilog.Kernel.Enums;

namespace Marilog.Contracts.DTOs.Requests.DocumentTemplateDTOs
{
    // One box on the template page. Used both ways (request and response) so the designer can edit it in place.
    // Positions and sizes are in PDF points from the page's top-left corner.
    public class DocumentTemplateElementDto
    {
        public TemplateElementKey Key { get; set; }
        public TemplatePageScope PageScope { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public float FontSize { get; set; } = 10;
        public bool Bold { get; set; }
        public TemplateTextAlign Align { get; set; }
        public bool Border { get; set; }
        public string? Color { get; set; }                // "#RRGGBB", null = black (items table: header text)
        public string? Background { get; set; }           // "#RRGGBB", null = transparent (items table: header row)
        public string? Text { get; set; }                 // StaticText
        public string? ImageDataUrl { get; set; }         // Image: "data:image/png;base64,..."
        public List<DocumentTemplateColumnDto> Columns { get; set; } = [];   // ItemsTable
    }

    public class DocumentTemplateColumnDto
    {
        public TemplateColumnKey Key { get; set; }
        public float Width { get; set; } = 1;             // relative to the other columns
        public string? Title { get; set; }
    }
}