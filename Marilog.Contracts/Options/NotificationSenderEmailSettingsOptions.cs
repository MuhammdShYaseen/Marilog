
namespace Marilog.Contracts.Options
{
    public sealed class NotificationSenderEmailSettingsOptions
    {
        // ── SMTP ────────────────────────────────────────────────────────────────

        public string SmtpHost { get; set; } = string.Empty;

        public int SmtpPort { get; set; }

        public bool SmtpUseSsl { get; set; }


        // ── IMAP ────────────────────────────────────────────────────────────────

        public string ImapHost { get; set; } = string.Empty;

        public int ImapPort { get; set; }

        public bool ImapUseSsl { get; set; }


        // ── Authentication ─────────────────────────────────────────────────────

        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;


        // ── Sender ──────────────────────────────────────────────────────────────

        public string FromEmail { get; set; } = string.Empty;

        public string? FromName { get; set; }
    }
}
