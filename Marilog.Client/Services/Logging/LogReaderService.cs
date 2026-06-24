using Marilog.Contracts.DTOs.LogDTOs;
using Marilog.Contracts.Interfaces.LogService;
using System.Net.Http.Json;
using System.Web;

namespace Marilog.Client.Services.Logging
{
    public sealed class LogReaderService : ILogReaderService
    {
        private readonly HttpClient _http;
        private const string Base = "api/logs";

        public LogReaderService(HttpClient http) => _http = http;

        public async Task<LogReadResult> QueryAsync(LogQuery query, CancellationToken ct = default)
        {
            var url = BuildQueryString(query);
            var result = await _http.GetFromJsonAsync<LogReadResult>(url, ct);
            return result ?? new LogReadResult([], 0, query.Page, query.PageSize, new(0, 0, 0, 0));
        }

        public async Task<IReadOnlyList<string>> GetAvailableFilesAsync(CancellationToken ct = default)
        {
            var result = await _http.GetFromJsonAsync<IReadOnlyList<string>>($"{Base}/files", ct);
            return result ?? [];
        }

        public async Task<LogStats?> GetStatsAsync(string? fileName = null, CancellationToken ct = default)
        {
            var url = fileName is null ? $"{Base}/stats" : $"{Base}/stats?fileName={Uri.EscapeDataString(fileName)}";
            return await _http.GetFromJsonAsync<LogStats>(url, ct);
        }

        // ── Private ───────────────────────────────────────────────────────────────
        private static string BuildQueryString(LogQuery q)
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(q.Levels))
                parts.Add($"levels={Uri.EscapeDataString(q.Levels)}");
            if (!string.IsNullOrWhiteSpace(q.Search))
                parts.Add($"search={Uri.EscapeDataString(q.Search)}");
            if (!string.IsNullOrWhiteSpace(q.HttpPath))
                parts.Add($"httpPath={Uri.EscapeDataString(q.HttpPath)}");
            if (!string.IsNullOrWhiteSpace(q.FileName))
                parts.Add($"fileName={Uri.EscapeDataString(q.FileName)}");
            if (q.HttpStatus.HasValue)
                parts.Add($"httpStatus={q.HttpStatus.Value}");
            if (q.From.HasValue)
                parts.Add($"from={q.From.Value:yyyy-MM-ddTHH:mm:ss}");
            if (q.To.HasValue)
                parts.Add($"to={q.To.Value:yyyy-MM-ddTHH:mm:ss}");

            parts.Add($"page={q.Page}");
            parts.Add($"pageSize={q.PageSize}");
            parts.Add($"sortBy={q.SortBy}");
            parts.Add($"descending={q.Descending.ToString().ToLower()}");

            return parts.Count > 0 ? $"{Base}?{string.Join("&", parts)}" : Base;
        }
    }
}