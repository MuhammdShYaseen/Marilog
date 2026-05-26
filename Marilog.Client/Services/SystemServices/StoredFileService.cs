using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.StoregFileDTOs;
using Marilog.Contracts.DTOs.Requests.TagDtos;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Kernel.Enums;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Marilog.Client.Services.SystemServices
{
    public class StoredFileService : IStoredFileService
    {
        private readonly HttpClient _http;
        private const string Base = "api/StoredFiles";

        public StoredFileService(HttpClient http) => _http = http;

        // ── Queries ──────────────────────────────────────────────────────────

        public async Task<StoredFileResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<StoredFileResponse>>($"{Base}/{id}", ct);
            return response?.Data;
        }

        public async Task<IReadOnlyList<StoredFileResponse>> GetByEntityIdAsync(
            int entityId,
            EntityType entityType,
            CancellationToken ct = default)
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<IReadOnlyList<StoredFileResponse>>>(
                $"{Base}/entity/{(int)entityType}/{entityId}", ct);
            return response?.Data ?? Array.Empty<StoredFileResponse>();
        }

        public async Task<PagedResponse<StoredFileResponse>> FullTextSearchAsync(
            string query,
            int page,
            int pageSize,
            EntityType entityType,
            CancellationToken ct = default)
        {
            var url = $"{Base}/search?query={Uri.EscapeDataString(query)}&page={page}&pageSize={pageSize}&entityType={(int)entityType}";
            var response = await _http.GetFromJsonAsync<ApiResponse<PagedResponse<StoredFileResponse>>>(url, ct);
            return response?.Data ?? new PagedResponse<StoredFileResponse>();
        }

        public async Task<IReadOnlyList<StoredFileResponse>> GetByTagsAsync(
            IReadOnlyList<string> tags,
            CancellationToken ct = default)
        {
            var query = string.Join("&", tags.Select(t => $"tags={Uri.EscapeDataString(t)}"));
            var response = await _http.GetFromJsonAsync<ApiResponse<IReadOnlyList<StoredFileResponse>>>(
                $"{Base}/by-tags?{query}", ct);
            return response?.Data ?? Array.Empty<StoredFileResponse>();
        }

        public async Task<Stream> GetFileStreamAsync(int id, CancellationToken ct = default)
        {
            return await _http.GetStreamAsync($"{Base}/{id}/stream", ct);
        }

        // ── Commands ─────────────────────────────────────────────────────────

        public async Task<StoredFileResponse> UploadAsync(
            UploadFileRequest request,
            CancellationToken ct = default)
        {
            using var content = new MultipartFormDataContent();
            using var streamContent = new StreamContent(request.FileStream);

            streamContent.Headers.ContentType = new MediaTypeHeaderValue(request.ContentType);

            content.Add(streamContent, "file", request.FileName);
            content.Add(new StringContent(((int)request.EntityType).ToString()), "entityType");

            if (request.EntityId is not null)
                content.Add(new StringContent(request.EntityId.ToString()!), "entityId");

            var httpResponse = await _http.PostAsync(Base, content, ct);
            httpResponse.EnsureSuccessStatusCode();

            var result = await httpResponse.Content
                .ReadFromJsonAsync<ApiResponse<StoredFileResponse>>(cancellationToken: ct);

            return result?.Data ?? throw new InvalidOperationException("Upload failed.");
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var httpResponse = await _http.DeleteAsync($"{Base}/{id}", ct);
            httpResponse.EnsureSuccessStatusCode();
        }

        public async Task UpdateEntityLinkAsync(
            int id,
            EntityType entityType,
            int? entityId,
            CancellationToken ct = default)
        {
            var request = new UpdateEntityLinkRequest { EntityType = entityType, EntityId = entityId };
            var httpResponse = await _http.PutAsJsonAsync($"{Base}/{id}/entity-link", request, ct);
            httpResponse.EnsureSuccessStatusCode();
        }

        public async Task UpdateContentFromOCRAsync(
            int id,
            string content,
            CancellationToken ct = default)
        {
            throw new NotSupportedException("This operation is reserved for the OCR Worker and cannot be called from the frontend.");
        }

        // ── Tags ─────────────────────────────────────────────────────────────

        public async Task AddTagAsync(
            int storedFileId,
            string name,
            string color,
            CancellationToken ct = default)
        {
            var request = new AddTagRequest { Name = name, Color = color };
            var httpResponse = await _http.PostAsJsonAsync($"{Base}/{storedFileId}/tags", request, ct);
            httpResponse.EnsureSuccessStatusCode();
        }

        public async Task RemoveTagAsync(int storedFileId, int tagId, CancellationToken ct = default)
        {
            var httpResponse = await _http.DeleteAsync($"{Base}/{storedFileId}/tags/{tagId}", ct);
            httpResponse.EnsureSuccessStatusCode();
        }
    }
}