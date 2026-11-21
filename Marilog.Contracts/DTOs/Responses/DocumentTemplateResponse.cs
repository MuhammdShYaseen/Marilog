using Marilog.Contracts.DTOs.Requests.DocumentTemplateDTOs;
using Marilog.Kernel.Enums;

namespace Marilog.Contracts.DTOs.Responses
{
    public class DocumentTemplateResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public TemplatePageSize PageSize { get; set; }
        public bool IsLandscape { get; set; }
        public string FontFamily { get; set; } = string.Empty;
        public List<DocumentTemplateElementDto> Elements { get; set; } = [];
        public DateTime? UpdatedAt { get; set; }
    }

    // Light list row: no elements (images can be large).
    public class DocumentTemplateSummaryResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public TemplatePageSize PageSize { get; set; }
        public bool IsLandscape { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    // A font file bundled with Marilog (Resources/Fonts). Url is where the browser loads it from.
    public class TemplateFontResponse
    {
        public string Family { get; set; } = string.Empty;
        public int Weight { get; set; }
        public bool IsItalic { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }
}