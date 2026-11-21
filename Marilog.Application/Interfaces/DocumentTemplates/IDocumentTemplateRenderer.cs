using Marilog.Contracts.DTOs.Responses;

namespace Marilog.Application.Interfaces.DocumentTemplates
{
    // Implemented in Infrastructure with QuestPDF.
    public interface IDocumentTemplateRenderer
    {
        // document == null → sample values (designer preview without a real document).
        byte[] Render(DocumentTemplateResponse template, DocumentResponse? document);

        // Fonts bundled in Resources/Fonts. Url is relative to the API base address.
        IReadOnlyList<TemplateFontResponse> GetFonts();

        // Full path of a bundled font file, or null if fileName isn't one of them.
        string? GetFontFilePath(string fileName);
    }
}