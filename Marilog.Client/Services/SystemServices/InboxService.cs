using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Reports.InboxEntryReport;
using Marilog.Contracts.DTOs.Requests.InboxEntryDTOs;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Kernel.Enums;
using System.Globalization;
using System.Net.Http.Json;

namespace Marilog.Client.Services.SystemServices
{
    public class InboxService : IInboxService
    {
        private readonly HttpClient _http;
        private const string Base = "api/inbox-entries";

        public InboxService(HttpClient http) => _http = http;

        // ── Queries ───────────────────────────────────────────────────────────────
        public async Task<InboxEntryResponse?> GetByIdAsync(int id, CancellationToken ct)
        {
            var http = await _http.GetCheckedAsync<InboxEntryResponse>($"{Base}/{id}", ct);
            return http;
        }
        public async Task<PagedResponse<InboxEntryResponse>> GetFilteredAsync(InboxEntryFilter filter,
            CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<PagedResponse<InboxEntryResponse>>(
                $"{Base}/filter{BuildQuery(filter)}", ct);

            return response ?? Empty(filter.Page, filter.PageSize);
        }

        public async Task<PagedResponse<InboxEntryResponse>> GetPagedAsync(PagedRequest request,
            CancellationToken ct = default)
        {
            var query = $"?page={request.Page}&pageSize={request.PageSize}";

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                query += $"&searchTerm={Uri.EscapeDataString(request.SearchTerm.Trim())}";

            var response = await _http.GetCheckedAsync<PagedResponse<InboxEntryResponse>>(
                $"{Base}{query}", ct);

            return response ?? Empty(request.Page, request.PageSize);
        }

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<string> CreateAsync(DateOnly receivedDate, InboxSource source,
            List<int> vesselIds, string description, int? startSequence = null,
            CancellationToken ct = default)
        {
            var request = new CreateInboxEntryRequest(receivedDate, source, vesselIds,
                description, startSequence);

            var http = await _http.PostAsJsonAsync(Base, request, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
            return (await http.Content.ReadFromJsonAsync<string>(ct))!;
        }


        public async Task<List<string>> CreateRangeAsync(List<CreateInboxEntryItem> items,  bool keepInputOrder = false, CancellationToken ct = default)
        {
            var http = await _http.PostAsJsonAsync($"{Base}/range?keepInputOrder={(keepInputOrder ? "true" : "false")}", items, ct);
            return await http.Content.ReadFromJsonAsync<List<string>>(ct) ?? [];
        }
        public async Task UpdateAsync(int id, DateOnly receivedDate, InboxSource source,
            List<int> vesselIds, string description, CancellationToken ct = default)
        {
            var request = new UpdateInboxEntryRequest(receivedDate, source, vesselIds, description);

            var http = await _http.PutAsJsonAsync($"{Base}/{id}", request, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
        }

        public async Task LinkAsync(int id, EntityType entityType, int entityId,
            CancellationToken ct = default)
        {
            var http = await _http.PostAsJsonAsync($"{Base}/{id}/links",
                new LinkInboxEntryRequest(entityType, entityId), ct);
            await http.EnsureSuccessWithMessageAsync(ct);
        }

        public async Task UnlinkAsync(int linkId, CancellationToken ct = default)
        {
            var http = await _http.DeleteAsync($"{Base}/links/{linkId}", ct);
            await http.EnsureSuccessWithMessageAsync(ct);
        }

        public async Task MarkProcessedAsync(int id, CancellationToken ct = default)
        {
            var http = await _http.PostAsync($"{Base}/{id}/processed", null, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
        }

        public async Task CancelAsync(int id, CancellationToken ct = default)
        {
            var http = await _http.PostAsync($"{Base}/{id}/cancel", null, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var http = await _http.DeleteAsync($"{Base}/{id}", ct);
            await http.EnsureSuccessWithMessageAsync(ct);
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private static string BuildQuery(InboxEntryFilter f)
        {
            var q = new List<string> { $"page={f.Page}", $"pageSize={f.PageSize}" };

            if (!string.IsNullOrWhiteSpace(f.SearchTerm))
                q.Add($"searchTerm={Uri.EscapeDataString(f.SearchTerm.Trim())}");

            if (f.Year is not null) q.Add($"year={f.Year}");
            if (f.From is not null) q.Add($"from={f.From.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");
            if (f.To is not null) q.Add($"to={f.To.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");
            if (f.Source is not null) q.Add($"source={f.Source}");
            if (f.VesselId is not null) q.Add($"vesselId={f.VesselId}");
            if (f.IsProcessed is not null) q.Add($"isProcessed={(f.IsProcessed.Value ? "true" : "false")}");
            if (f.IsCancelled is not null) q.Add($"isCancelled={(f.IsCancelled.Value ? "true" : "false")}");
            if (f.LinkedEntityType is not null) q.Add($"linkedEntityType={f.LinkedEntityType}");
            if (f.LinkedEntityId is not null) q.Add($"linkedEntityId={f.LinkedEntityId}");

            return "?" + string.Join("&", q);
        }

        private static PagedResponse<InboxEntryResponse> Empty(int page, int pageSize)
            => new() { Page = page, PageSize = pageSize };

       
    }
}
