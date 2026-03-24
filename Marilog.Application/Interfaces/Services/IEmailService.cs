using Marilog.Domain.Entities;
using Marilog.Domain.Events;

namespace Marilog.Application.Interfaces.Services
{
    public interface IEmailService
    {
        // ── Queries ───────────────────────────────────────────────────────────────
        Task<Email?>              GetByIdAsync(int id, CancellationToken ct = default);
        Task<Email?>              GetFullAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<Email>> GetByEntityAsync(string entityType, int entityId, CancellationToken ct = default);
        Task<IReadOnlyList<Email>> GetByStatusAsync(EmailStatus status, CancellationToken ct = default);
        Task<IReadOnlyList<Email>> GetByParticipantAsync(ParticipantType participantType, int participantId, CancellationToken ct = default);

        // ── Commands ─────────────────────────────────────────────────────────────
        Task<Email> CreateAsync(string entityType, int entityId, string subject, string body,
                                EmailDirection direction, IReadOnlyList<EmailParticipantData> participants,
                                CancellationToken ct = default);
        Task        MarkAsSentAsync(int id, DateTime sentAt, string? externalId = null, CancellationToken ct = default);
        Task        MarkAsReceivedAsync(int id, CancellationToken ct = default);
        Task        MarkAsFailedAsync(int id, CancellationToken ct = default);
        Task        RetryAsync(int id, CancellationToken ct = default);
        Task        DeleteAsync(int id, CancellationToken ct = default);

        // ── Participants ──────────────────────────────────────────────────────────
        Task<EmailParticipant> AddParticipantAsync(int emailId, ParticipantRole role,
                                                   ParticipantType participantType,
                                                   int participantId,
                                                   string? displayName = null,
                                                   string? emailAddress = null,
                                                   CancellationToken ct = default);
        Task                   RemoveParticipantAsync(int emailId, int participantId,
                                                      CancellationToken ct = default);

        // ── Attachments ───────────────────────────────────────────────────────────
        Task<EmailAttachment> AddAttachmentAsync(int emailId, string fileName,
                                                 string filePath, long fileSizeBytes,
                                                 CancellationToken ct = default);
        Task                  RemoveAttachmentAsync(int emailId, int attachmentId,
                                                    CancellationToken ct = default);
    }
}
