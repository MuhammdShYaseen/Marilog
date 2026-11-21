using Marilog.Contracts.DTOs.Requests.DocumentTemplateDTOs;
using Marilog.Contracts.DTOs.Responses;

namespace Marilog.Contracts.Interfaces.Services.SystemServices
{
    public interface IDocumentTemplateService
    {
        // ── Queries ───────────────────────────────────────────────────────────────
        Task<IReadOnlyList<DocumentTemplateSummaryResponse>> GetAllAsync(CancellationToken ct = default);
        Task<DocumentTemplateResponse?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<TemplateFontResponse>> GetFontsAsync(CancellationToken ct = default);

        // ── Commands ──────────────────────────────────────────────────────────────
        Task<DocumentTemplateResponse> CreateAsync(SaveDocumentTemplateRequest request, CancellationToken ct = default);
        Task<DocumentTemplateResponse> UpdateAsync(int id, SaveDocumentTemplateRequest request, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);

        // ── PDF ───────────────────────────────────────────────────────────────────
        Task<byte[]> ExportAsync(int templateId, int documentId, CancellationToken ct = default);
        Task<byte[]> PreviewAsync(PreviewDocumentTemplateRequest request, CancellationToken ct = default);
    }
}