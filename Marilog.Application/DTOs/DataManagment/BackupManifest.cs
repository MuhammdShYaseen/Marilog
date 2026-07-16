

namespace Marilog.Application.DTOs.DataManagment
{
    public record BackupManifest
    {
        public DateTime CreatedAtUtc { get; init; }
        public string SchemaVersion { get; init; } = "";
        public string DatabaseProvider { get; init; } = "";
        public string DatabaseSha256 { get; init; } = "";
        public string FilesSha256 { get; init; } = "";
    }
}
