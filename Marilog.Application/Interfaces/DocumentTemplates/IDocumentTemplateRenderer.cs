using Marilog.Contracts.DTOs.Responses;

namespace Marilog.Application.Interfaces.DocumentTemplates
{
    // Implemented in Infrastructure with QuestPDF.
    public interface IDocumentTemplateRenderer
    {
        // images: picture bytes by the element's index in template.Elements (the service loads them from StoredFiles).
        byte[] Render(DocumentTemplateResponse template, DocumentResponse? document, IReadOnlyDictionary<int, byte[]> images);

        // Fonts bundled in Resources/Fonts. Url is relative to the API base address.
        IReadOnlyList<TemplateFontResponse> GetFonts();

        // Full path of a bundled font file, or null if fileName isn't one of them.
        string? GetFontFilePath(string fileName);
    }
}