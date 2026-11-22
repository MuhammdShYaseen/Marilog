using Marilog.Contracts.DTOs.Responses;

namespace Marilog.Application.Interfaces.DocumentTemplates
{

    public interface IDocumentTemplateRenderer
    {
        // data = null → sample values (designer preview without a document).
        byte[] Render(DocumentTemplateResponse template, DocumentTemplateData? data, IReadOnlyDictionary<int, byte[]> images);
        IReadOnlyList<TemplateFontResponse> GetFonts();
        string? GetFontFilePath(string fileName);
    }
}