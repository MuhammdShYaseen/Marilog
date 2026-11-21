using Marilog.Client.Extensions;
using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.DocumentTemplateDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using System.Net.Http.Json;

namespace Marilog.Client.Services.SystemServices
{
    public class DocumentTemplateService : IDocumentTemplateService
    {
        private readonly HttpClient _http;
        private const string Base = "api/DocumentTemplates";

        public DocumentTemplateService(HttpClient http) => _http = http;

        // ── Queries ──────────────────────────────────────────────────────────

        public async Task<IReadOnlyList<DocumentTemplateSummaryResponse>> GetAllAsync(CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<ApiResponse<IReadOnlyList<DocumentTemplateSummaryResponse>>>(Base, ct);
            return response?.Data ?? Array.Empty<DocumentTemplateSummaryResponse>();
        }

        public async Task<DocumentTemplateResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<ApiResponse<DocumentTemplateResponse>>($"{Base}/{id}", ct);
            return response?.Data;
        }

        public async Task<IReadOnlyList<TemplateFontResponse>> GetFontsAsync(CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<ApiResponse<IReadOnlyList<TemplateFontResponse>>>($"{Base}/fonts", ct);
            var fonts = response?.Data ?? Array.Empty<TemplateFontResponse>();

            // The server sends a path relative to the API; the browser needs the full address for @font-face.
            foreach (var font in fonts)
                font.Url = new Uri(_http.BaseAddress!, font.Url).ToString();

            return fonts;
        }

        // ── Commands ─────────────────────────────────────────────────────────

        public async Task<DocumentTemplateResponse> CreateAsync(SaveDocumentTemplateRequest request, CancellationToken ct = default)
        {
            var httpResponse = await _http.PostAsJsonAsync(Base, request, ct);
            await httpResponse.EnsureSuccessWithMessageAsync(ct);

            var response = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<DocumentTemplateResponse>>(cancellationToken: ct);
            return response?.Data ?? throw new InvalidOperationException("Template creation failed.");
        }

        public async Task<DocumentTemplateResponse> UpdateAsync(int id, SaveDocumentTemplateRequest request, CancellationToken ct = default)
        {
            var httpResponse = await _http.PutAsJsonAsync($"{Base}/{id}", request, ct);
            await httpResponse.EnsureSuccessWithMessageAsync(ct);

            var response = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<DocumentTemplateResponse>>(cancellationToken: ct);
            return response?.Data ?? throw new InvalidOperationException("Template update failed.");
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var httpResponse = await _http.DeleteAsync($"{Base}/{id}", ct);
            await httpResponse.EnsureSuccessWithMessageAsync(ct);
        }

        // ── PDF ──────────────────────────────────────────────────────────────

        public async Task<byte[]> ExportAsync(int templateId, int documentId, CancellationToken ct = default)
        {
            using var response = await _http.GetAsync($"{Base}/{templateId}/export/{documentId}", ct);
            await response.EnsureSuccessWithMessageAsync(ct);
            return await response.Content.ReadAsByteArrayAsync(ct);
        }

        public async Task<byte[]> PreviewAsync(PreviewDocumentTemplateRequest request, CancellationToken ct = default)
        {
            using var response = await _http.PostAsJsonAsync($"{Base}/preview", request, ct);
            await response.EnsureSuccessWithMessageAsync(ct);
            return await response.Content.ReadAsByteArrayAsync(ct);
        }
    }
}