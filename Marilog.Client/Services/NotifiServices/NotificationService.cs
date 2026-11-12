using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.NotificationDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.NotifiServices;
using Marilog.Kernel.Enums;
using System.Net.Http.Json;

namespace Marilog.Client.Services.NotifiServices
{
    public class NotificationService : INotificationService
    {
        private readonly HttpClient _http;
        private const string Base = "api/notifications";

        public NotificationService(HttpClient http) => _http = http;

        // ── Follow ───────────────────────────────────────────────────────────

        public async Task<bool> IsFollowingAsync(EntityType type, int id, CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<ApiResponse<bool>>($"{Base}/watch/{(int)type}/{id}", ct);
            return response?.Data ?? false;
        }

        public async Task<bool> FollowAsync(FollowRequest request, CancellationToken ct = default)
        {
            var httpResponse = await _http.PostAsJsonAsync($"{Base}/watch", request, ct);
            await httpResponse.EnsureSuccessWithMessageAsync(ct);
            return true;
        }

        public async Task UnfollowAsync(EntityType type, int id, CancellationToken ct = default)
        {
            var httpResponse = await _http.DeleteAsync($"{Base}/watch/{(int)type}/{id}", ct);
            await httpResponse.EnsureSuccessWithMessageAsync(ct);
        }

        // ── Notifications ────────────────────────────────────────────────────
        // Permissions are applied by the server; readableTypes is ignored here.

        public async Task<IReadOnlyList<NotificationResponse>> GetMineAsync(IReadOnlyCollection<EntityType>? readableTypes = null, CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<ApiResponse<IReadOnlyList<NotificationResponse>>>(Base, ct);
            return response?.Data ?? Array.Empty<NotificationResponse>();
        }

        public async Task<int> GetUnreadCountAsync(IReadOnlyCollection<EntityType>? readableTypes = null, CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<ApiResponse<int>>($"{Base}/unread-count", ct);
            return response?.Data ?? 0;
        }

        public async Task MarkReadAsync(long id, CancellationToken ct = default)
        {
            var httpResponse = await _http.PostAsync($"{Base}/{id}/mark-read", null, ct);
            await httpResponse.EnsureSuccessWithMessageAsync(ct);
        }

        public async Task MarkAllReadAsync(CancellationToken ct = default)
        {
            var httpResponse = await _http.PostAsync($"{Base}/mark-all-read", null, ct);
            await httpResponse.EnsureSuccessWithMessageAsync(ct);
        }
    }
}