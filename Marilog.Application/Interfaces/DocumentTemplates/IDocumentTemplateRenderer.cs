using Marilog.Contracts.DTOs.Responses;

namespace Marilog.Application.Interfaces.DocumentTemplates
{

    public interface IDocumentTemplateRenderer
    {
        byte[] Render(DocumentTemplateResponse template, DocumentResponse? document, IReadOnlyDictionary<int, byte[]> images);
        IReadOnlyList<TemplateFontResponse> GetFonts();
        string? GetFontFilePath(string fileName);
    }
}