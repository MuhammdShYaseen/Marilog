using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.AuditDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.AuditLog;
using System.Globalization;
using System.Net.Http.Json;

namespace Marilog.Client.Services.Audit
{
    public class AuditLogService : IAuditLogService
    {
        private readonly HttpClient _http;
        private const string Base = "api/auditlog";

        public AuditLogService(HttpClient http) => _http = http;

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<PagedResponse<AuditLogListItem>> GetPagedAsync(
            PagedRequest paging, AuditLogFilter filter, CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<ApiResponse<PagedResponse<AuditLogListItem>>>(
                $"{Base}?{BuildQuery(paging, filter)}", ct);

            return response?.Data ?? new PagedResponse<AuditLogListItem>
            {
                Page = paging.Page,
                PageSize = paging.PageSize
            };
        }

        public async Task<AuditLogResponse?> GetByIdAsync(long id, CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<ApiResponse<AuditLogResponse>>($"{Base}/{id}", ct);
            return response?.Data;
        }

        public async Task<AuditSettingsOptions> GetSettingsAsync(CancellationToken ct = default)
        {
            var response = await _http.GetCheckedAsync<ApiResponse<AuditSettingsOptions>>($"{Base}/settings", ct);

            // Never fall back to defaults: showing 365 when the real value differs would mislead the admin
            return response?.Data
                ?? throw new InvalidOperationException("Audit settings could not be loaded.");
        }

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<PurgeAuditLogsResponse> PurgeAsync(DateOnly? before, CancellationToken ct = default)
        {
            var url = before.HasValue
                ? $"{Base}/purge?before={FormatDate(before.Value)}"
                : $"{Base}/purge";

            var http = await _http.DeleteAsync(url, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
            var response = await http.Content.ReadFromJsonAsync<ApiResponse<PurgeAuditLogsResponse>>(ct);
            return response!.Data!;
        }

        public async Task UpdateSettingsAsync(AuditSettingsOptions request, CancellationToken ct = default)
        {
            var http = await _http.PutAsJsonAsync($"{Base}/settings", request, ct);
            await http.EnsureSuccessWithMessageAsync(ct);
        }

        // ── Private Helpers ───────────────────────────────────────────────────────

        private static string BuildQuery(PagedRequest paging, AuditLogFilter filter)
        {
            var parts = new List<string>
            {
                $"page={paging.Page}",
                $"pageSize={paging.PageSize}"
            };

            if (!string.IsNullOrWhiteSpace(paging.SearchTerm))
                parts.Add($"searchTerm={Uri.EscapeDataString(paging.SearchTerm.Trim())}");

            // Enums as numbers: the API binds both, numbers survive enum renames
            if (filter.EntityType.HasValue)
                parts.Add($"entityType={(int)filter.EntityType.Value}");

            if (filter.EntityId.HasValue)
                parts.Add($"entityId={filter.EntityId.Value}");

            if (filter.UserId.HasValue)
                parts.Add($"userId={filter.UserId.Value}");

            if (filter.Operation.HasValue)
                parts.Add($"operation={(int)filter.Operation.Value}");

            if (filter.From.HasValue)
                parts.Add($"from={FormatDate(filter.From.Value)}");

            if (filter.To.HasValue)
                parts.Add($"to={FormatDate(filter.To.Value)}");

            return string.Join("&", parts);
        }

        // Invariant culture: an Arabic UI culture would otherwise produce Arabic-Indic digits
        private static string FormatDate(DateOnly date)
            => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }
}