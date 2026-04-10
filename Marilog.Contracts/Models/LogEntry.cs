namespace Marilog.Contracts.Models
{
    public class LogEntry
    {
        public int LineNumber { get; init; }
        public DateTime Timestamp { get; init; }
        public string Level { get; init; } = default!;
        public string Message { get; init; } = default!;
        public string? ExceptionType { get; init; }
        public string? ExceptionMessage { get; init; }
        public string? StackTrace { get; init; }
        public string? ErrorId { get; init; }
        public string? HttpPath { get; init; }
        public string? HttpMethod { get; init; }
        public int? HttpStatus { get; init; }
        public double? DurationMs { get; init; }
    }
}
