

namespace Marilog.Contracts.DTOs.Requests.AuditDTOs
{
    public sealed class AuditSettingsOptions
    {
        // Hard floor enforced in code: the settings file can never go below it
        public const int MinRetentionDays = 30;
        public const int DefaultRetentionDays = 365;

        public int RetentionDays { get; set; } = DefaultRetentionDays;
    }
}
