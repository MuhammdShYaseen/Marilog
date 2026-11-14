using Marilog.Kernel.Enums;

namespace Marilog.Contracts.DTOs.Responses
{
    public sealed class BackupJobResponse
    {
        public string Token { get; init; } = string.Empty;
        public BackupJobStatus Status { get; init; }
        public string FileName { get; init; } = string.Empty;
        public long SizeBytes { get; init; }
        public string? Error { get; init; }
    }
}