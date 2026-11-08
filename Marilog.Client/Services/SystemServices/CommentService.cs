using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.CommentDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Kernel.Enums;
using System.Net.Http.Json;

namespace Marilog.Client.Services.SystemServices
{
    public class CommentService : ICommentService
    {
        private readonly HttpClient _http;
        private const string Base = "api/Comments";

        public CommentService(HttpClient http) => _http = http;

        // ── Queries ──────────────────────────────────────────────────────────

        public async Task<CommentResponse?> GetByIdAsync(
            int id,
            CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<ApiResponse<CommentResponse>>(
                $"{Base}/{id}", ct);

            return response?.Data;
        }

        public async Task<PagedResponse<CommentResponse>> GetByUserIdAsync(
            Guid userId,
            PagedRequest request,
            CancellationToken ct = default)
        {
            var url = $"{Base}/user/{userId}?page={request.Page}&pageSize={request.PageSize}";

            var response = await _http.GetCheckedAsync<ApiResponse<PagedResponse<CommentResponse>>>(
                url, ct);

            return response?.Data ?? new PagedResponse<CommentResponse>();
        }

        public async Task<IReadOnlyList<CommentResponse>> GetByEntityAsync(
            EntityType entityType,
            int entityId,
            CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<ApiResponse<IReadOnlyList<CommentResponse>>>(
                $"{Base}/entity/{(int)entityType}/{entityId}", ct);

            return response?.Data ?? Array.Empty<CommentResponse>();
        }

        public async Task<PagedResponse<CommentResponse>> GetAllAsync(
            PagedRequest request,
            CancellationToken ct = default)
        {
            var url = $"{Base}?page={request.Page}&pageSize={request.PageSize}";

            var response = await _http.GetCheckedAsync<ApiResponse<PagedResponse<CommentResponse>>>(
                url, ct);

            return response?.Data ?? new PagedResponse<CommentResponse>();
        }

        // ── Commands ─────────────────────────────────────────────────────────

        public async Task<CommentResponse> CreateAsync(CreateCommentRequest request, CancellationToken ct = default)
        {
            // UserId is determined by the server from the authenticated JWT.

            var httpResponse = await _http.PostAsJsonAsync(Base, request, ct);
            await httpResponse.EnsureSuccessWithMessageAsync(ct);

            var response = await httpResponse.Content
                .ReadFromJsonAsync<ApiResponse<CommentResponse>>(cancellationToken: ct);

            return response?.Data
                ?? throw new InvalidOperationException("Comment creation failed.");
        }

        public async Task<CommentResponse> UpdateAsync(
            int id,
            UpdateCommentRequest request,
            CancellationToken ct = default)
        {
            var httpResponse = await _http.PutAsJsonAsync($"{Base}/{id}", request, ct);
            await httpResponse.EnsureSuccessWithMessageAsync(ct);

            var response = await httpResponse.Content
                .ReadFromJsonAsync<ApiResponse<CommentResponse>>(cancellationToken: ct);

            return response?.Data
                ?? throw new InvalidOperationException("Comment update failed.");
        }

        public async Task DeleteAsync(
            int id,
            CancellationToken ct = default)
        {
            var httpResponse = await _http.DeleteAsync($"{Base}/{id}", ct);
            await httpResponse.EnsureSuccessWithMessageAsync(ct);
        }
    }
}