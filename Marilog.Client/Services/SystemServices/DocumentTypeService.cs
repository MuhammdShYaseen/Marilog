using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.DocumentTypeDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using System.Net.Http.Json;

namespace Marilog.Client.Services.SystemServices
{
    public class DocumentTypeService : IDocumentTypeService
    {
        private readonly HttpClient _http;
        private const string Base = "api/document-types";

        public DocumentTypeService(HttpClient http) => _http = http;

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<DocumentTypeResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            // Controller returns unwrapped on this endpoint
            var response = await _http.GetFromJsonAsync<ApiResponse<DocumentTypeResponse>>($"{Base}/{id}", ct);
            return response?.Data;
        }

        public async Task<DocumentTypeResponse?> GetByCodeAsync(string code, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<DocumentTypeResponse>>($"{Base}/by-code/{Uri.EscapeDataString(code)}", ct);
            return response?.Data;
        }

        public async Task<IReadOnlyList<DocumentTypeResponse>> GetAllAsync(CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<IReadOnlyList<DocumentTypeResponse>>>(Base, ct);
            return response?.Data ?? [];
        }

        public async Task<IReadOnlyList<DocumentTypeResponse>> GetActiveAsync(CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<IReadOnlyList<DocumentTypeResponse>>>($"{Base}/active", ct);
            return response?.Data ?? [];
        }

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<DocumentTypeResponse> CreateAsync(string code, string name, int sortOrder = 0, CancellationToken ct = default)
        {
            var request = new CreateDocumentTypeRequest { Code = code, Name = name, SortOrder = sortOrder };
            var http = await _http.PostAsJsonAsync(Base, request, ct);
            http.EnsureSuccessStatusCode();
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<DocumentTypeResponse>>(ct);
            return response!.Data!;
        }

        public async Task<IReadOnlyList<DocumentTypeResponse>> CreateRangeAsync(IEnumerable<CreateDocumentTypeRequest> commands, CancellationToken ct = default)
        {
            var http = await _http.PostAsJsonAsync($"{Base}/batch", commands, ct);
            http.EnsureSuccessStatusCode();
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<DocumentTypeResponse>>>(ct);
            return response?.Data ?? [];
        }

        public async Task UpdateAsync(int id, string name, int sortOrder, CancellationToken ct = default)
        {
            var request = new UpdateDocumentTypeRequest { Name = name, SortOrder = sortOrder };
            var http = await _http.PutAsJsonAsync($"{Base}/{id}", request, ct);
            http.EnsureSuccessStatusCode();
        }

        public async Task ActivateAsync(int id, CancellationToken ct = default)
        {
            var http = await _http.PatchAsync($"{Base}/{id}/activate", null, ct);
            http.EnsureSuccessStatusCode();
        }

        public async Task DeactivateAsync(int id, CancellationToken ct = default)
        {
            var http = await _http.PatchAsync($"{Base}/{id}/deactivate", null, ct);
            http.EnsureSuccessStatusCode();
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var http = await _http.DeleteAsync($"{Base}/{id}", ct);
            http.EnsureSuccessStatusCode();
        }
    }
}