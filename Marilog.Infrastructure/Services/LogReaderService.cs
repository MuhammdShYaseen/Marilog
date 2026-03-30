using Marilog.Application.DTOs.LogDTOs;
using Marilog.Application.DTOs.Logs;
using Marilog.Application.Interfaces.LogInterfaces;
using Marilog.Domain.ReadModels;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Marilog.Infrastructure.Services
{
    public sealed partial class LogReaderService : ILogReaderService
    {
        // يطابق: 2026-03-29 11:16:56 [ERR] HTTP GET /api/... responded 500 in 103.2ms
        [GeneratedRegex(
            @"^(?<ts>\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}) \[(?<lvl>ERR|WRN|INF|DBG)\] (?<msg>.+)$",
            RegexOptions.Compiled)]
        private static partial Regex EntryLine();

        [GeneratedRegex(
            @"HTTP (?<method>\w+) (?<path>/\S+) responded (?<status>\d+) in (?<dur>[\d.]+)ms",
            RegexOptions.Compiled)]
        private static partial Regex HttpLine();

        [GeneratedRegex(@"ErrorId: (?<id>[\w\-]+)", RegexOptions.Compiled)]
        private static partial Regex ErrorIdLine();

        [GeneratedRegex(@"^(?<type>[\w.]+Exception): (?<msg>.+)$", RegexOptions.Compiled)]
        private static partial Regex ExceptionLine();

        private readonly string _logDirectory;

        public LogReaderService(IConfiguration config)
        {
            _logDirectory = config["Logging:Directory"] ?? "logs";
        }

        public Task<IReadOnlyList<string>> GetAvailableFilesAsync(CancellationToken ct = default)
        {
            if (!Directory.Exists(_logDirectory))
                return Task.FromResult<IReadOnlyList<string>>([]);

            var files = Directory.GetFiles(_logDirectory, "*.log")
                .Select(Path.GetFileName)
                .Where(f => f is not null)
                .Cast<string>()
                .OrderDescending()
                .ToList();

            return Task.FromResult<IReadOnlyList<string>>(files);
        }

        public async Task<LogReadResult> QueryAsync(LogQuery query, CancellationToken ct = default)
        {
            var filePath = ResolveFile(query.FileName);

            if (filePath is null || !File.Exists(filePath))
                return Empty(query);

            var entries = await ParseFileAsync(filePath, ct);

            // ── Filtering ──────────────────────────────────────────
            var filtered = entries.AsEnumerable();

            if (query.LevelList.Any())
                filtered = filtered.Where(e => query.LevelList.Contains(e.Level));

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var q = query.Search.Trim();
                filtered = filtered.Where(e =>
                    e.Message.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    (e.ExceptionType?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (e.ExceptionMessage?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (e.StackTrace?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false));
            }

            if (query.From.HasValue) filtered = filtered.Where(e => e.Timestamp >= query.From.Value);
            if (query.To.HasValue) filtered = filtered.Where(e => e.Timestamp <= query.To.Value);
            if (!string.IsNullOrWhiteSpace(query.HttpPath))
                filtered = filtered.Where(e => e.HttpPath?.Contains(
                    query.HttpPath, StringComparison.OrdinalIgnoreCase) ?? false);
            if (query.HttpStatus.HasValue)
                filtered = filtered.Where(e => e.HttpStatus == query.HttpStatus);

            // ── Sorting ────────────────────────────────────────────
            filtered = query.SortBy.ToLowerInvariant() switch
            {
                "level" => query.Descending
                    ? filtered.OrderByDescending(e => e.Level)
                    : filtered.OrderBy(e => e.Level),
                _ => query.Descending
                    ? filtered.OrderByDescending(e => e.Timestamp)
                    : filtered.OrderBy(e => e.Timestamp)
            };

            var materialized = filtered.ToList();

            // ── Stats ──────────────────────────────────────────────
            var stats = new LogStats(
                Errors: materialized.Count(e => e.Level == "ERR"),
                Warnings: materialized.Count(e => e.Level == "WRN"),
                Info: materialized.Count(e => e.Level == "INF"),
                Debug: materialized.Count(e => e.Level == "DBG")
            );

            // ── Pagination ─────────────────────────────────────────
            var page = Math.Max(1, query.Page);
            var pageSize = Math.Clamp(query.PageSize, 1, 500);
            var items = materialized
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new LogReadResult(items, materialized.Count, page, pageSize, stats);
        }

        // ── Private helpers ────────────────────────────────────────

        private string? ResolveFile(string? fileName)
        {
            if (!Directory.Exists(_logDirectory)) return null;

            if (!string.IsNullOrWhiteSpace(fileName))
                return Path.Combine(_logDirectory, Path.GetFileName(fileName)!);

            // أحدث ملف
            return Directory.GetFiles(_logDirectory, "*.log")
                .OrderDescending()
                .FirstOrDefault();
        }

        private async Task<List<LogEntry>> ParseFileAsync(string path, CancellationToken ct)
        {
            // قراءة بدون lock على الملف (مهم لأن Serilog يكتب فيه)
            await using var fs = new FileStream(
                path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(fs);

            var entries = new List<LogEntry>();
            LogEntry? current = null;
            var stackBuf = new List<string>();
            int lineNum = 0;

            while (await reader.ReadLineAsync(ct) is { } line)
            {
                lineNum++;
                var m = EntryLine().Match(line);

                if (m.Success)
                {
                    if (current is not null)
                        entries.Add(Finalize(current, stackBuf));

                    stackBuf.Clear();
                    current = BuildEntry(m, lineNum);
                }
                else if (current is not null && !string.IsNullOrWhiteSpace(line))
                {
                    stackBuf.Add(line);
                    EnrichFromStackLine(current, line, ref current);
                }
            }

            if (current is not null)
                entries.Add(Finalize(current, stackBuf));

            return entries;
        }

        private LogEntry BuildEntry(Match m, int lineNum)
        {
            var msg = m.Groups["msg"].Value;
            var httpM = HttpLine().Match(msg);
            var errorIdM = ErrorIdLine().Match(msg);

            return new LogEntry
            {
                LineNumber = lineNum,
                Timestamp = DateTime.Parse(m.Groups["ts"].Value),
                Level = m.Groups["lvl"].Value,
                Message = msg,
                HttpMethod = httpM.Success ? httpM.Groups["method"].Value : null,
                HttpPath = httpM.Success ? httpM.Groups["path"].Value
                            : errorIdM.Success
                                ? ExtractPath(msg)
                                : null,
                HttpStatus = httpM.Success ? int.Parse(httpM.Groups["status"].Value) : null,
                DurationMs = httpM.Success ? double.Parse(httpM.Groups["dur"].Value) : null,
                ErrorId = errorIdM.Success ? errorIdM.Groups["id"].Value : null,
            };
        }

        // ref trick لتعديل record بعد الإنشاء (records are immutable, نعيد بناء)
        private static void EnrichFromStackLine(LogEntry entry, string line, ref LogEntry current)
        {
            var exM = ExceptionLine().Match(line.Trim());
            if (exM.Success && current.ExceptionType is null)
            {
                current = current with
                {
                    ExceptionType = exM.Groups["type"].Value.Split('.').Last(),
                    ExceptionMessage = exM.Groups["msg"].Value
                };
            }
        }

        private static LogEntry Finalize(LogEntry entry, List<string> stack) =>
            stack.Count == 0 ? entry
                : entry with { StackTrace = string.Join('\n', stack) };

        private static string? ExtractPath(string msg)
        {
            var m = Regex.Match(msg, @"Path: (/\S+)");
            return m.Success ? m.Groups[1].Value : null;
        }

        private static LogReadResult Empty(LogQuery q) =>
            new([], 0, q.Page, q.PageSize, new(0, 0, 0, 0));
    }
}
