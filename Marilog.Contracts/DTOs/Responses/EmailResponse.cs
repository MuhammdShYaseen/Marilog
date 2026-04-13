
using Marilog.Contracts.Enums;

namespace Marilog.Contracts.DTOs.Responses
{
    public class EmailResponse
    {
        public string EntityType { get; private set; } = null!;  // "Document" | "SwiftTransfer" | "Voyage"
        public int EntityId { get; private set; }

        // ── Content ───────────────────────────────────────────────────────────────
        public string Subject { get; private set; } = null!;
        public string Body { get; private set; } = null!;

        // ── Meta ──────────────────────────────────────────────────────────────────
        public EmailDirection Direction { get; private set; }
        public EmailStatus Status { get; private set; } = EmailStatus.Draft;
        public DateTime? SentAt { get; private set; }
        public string? ExternalId { get; private set; }  // Message-ID from mail server

        

        public IReadOnlyCollection<EmailParticipantData> Participants { get; init; } = new List<EmailParticipantData>();
        public IReadOnlyCollection<EmailAttachmentResponse> Attachments { get; init; } = new List<EmailAttachmentResponse>();
    }
}
