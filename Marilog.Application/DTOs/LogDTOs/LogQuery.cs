

namespace Marilog.Application.DTOs.Logs
{
    public sealed class LogQuery
    {
        /// <summary>ERR | WRN | INF | DBG — comma-separated للمتعدد</summary>
        public string? Levels { get; init; }

        public string? Search { get; init; }   // بحث نصي حر
        public DateTime? From { get; init; }
        public DateTime? To { get; init; }

        public string? HttpPath { get; init; }   // /api/companies
        public int? HttpStatus { get; init; }   // 500

        /// <summary>اسم ملف محدد — null يعني أحدث ملف</summary>
        public string? FileName { get; init; }

        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 50;

        public string SortBy { get; init; } = "timestamp"; // timestamp | level
        public bool Descending { get; init; } = true;

        public IEnumerable<string> LevelList =>
            string.IsNullOrWhiteSpace(Levels)
                ? []
                : Levels.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .Select(l => l.ToUpperInvariant());
    }
}
