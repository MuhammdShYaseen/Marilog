
using Marilog.Kernel.Enums;


namespace Marilog.Contracts.DTOs.Responses
{
    public class EmailResponse
    {
        public int Id { get; set; }
        public string EntityType { get;  set; } = null!;  // "Document" | "SwiftTransfer" | "Voyage"
        public int EntityId { get;  set; }

        // ── Content ───────────────────────────────────────────────────────────────
        public string Subject { get;  set; } = null!;
        public string Body { get;  set; } = null!;

        // ── Meta ──────────────────────────────────────────────────────────────────
        public EmailDirection Direction { get;  set; }
        public EmailStatus Status { get; set; } = EmailStatus.Draft;
        public DateTime? SentAt { get; set; }
        public string? ExternalId { get; set; }  // Message-ID from mail server

        

        public IReadOnlyCollection<EmailParticipantResponse> Participants { get; init; } = new List<EmailParticipantResponse>();
        public IReadOnlyCollection<EmailAttachmentResponse> Attachments { get; init; } = new List<EmailAttachmentResponse>();
    }
}
