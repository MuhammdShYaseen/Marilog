using Marilog.Contracts.DTOs.Responses;
using Marilog.Kernel.Enums;


namespace Marilog.Contracts.Interfaces.Services
{
    public interface IEmailService
    {
        // ── Queries ───────────────────────────────────────────────────────────────
        Task<EmailResponse?>              GetByIdAsync(int id, CancellationToken ct = default);
        Task<EmailResponse?>              GetFullAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<EmailResponse>> GetByEntityAsync(string entityType, int entityId, CancellationToken ct = default);
        Task<IReadOnlyList<EmailResponse>> GetByStatusAsync(EmailStatus status, CancellationToken ct = default);
        Task<IReadOnlyList<EmailResponse>> GetByParticipantAsync(ParticipantType participantType, int participantId, CancellationToken ct = default);

        // ── Commands ─────────────────────────────────────────────────────────────
        Task<EmailResponse> CreateAsync(string entityType, int entityId, string subject, string body,
                                EmailDirection direction, IReadOnlyList<EmailParticipantData> participants,
                                CancellationToken ct = default);
        Task        MarkAsSentAsync(int id, DateTime sentAt, string? externalId = null, CancellationToken ct = default);
        Task        MarkAsReceivedAsync(int id, CancellationToken ct = default);
        Task        MarkAsFailedAsync(int id, CancellationToken ct = default);
        Task        RetryAsync(int id, CancellationToken ct = default);
        Task        DeleteAsync(int id, CancellationToken ct = default);

        // ── Participants ──────────────────────────────────────────────────────────
        Task<EmailParticipantData> AddParticipantAsync(int emailId, ParticipantRole role,
                                                   ParticipantType participantType,
                                                   int participantId,
                                                   string? displayName = null,
                                                   string? emailAddress = null,
                                                   CancellationToken ct = default);
        Task                   RemoveParticipantAsync(int emailId, int participantId,
                                                      CancellationToken ct = default);

        // ── Attachments ───────────────────────────────────────────────────────────
        Task<EmailAttachmentResponse> AddAttachmentAsync(int emailId, string fileName,
                                                 string filePath, long fileSizeBytes,
                                                 CancellationToken ct = default);
        Task                  RemoveAttachmentAsync(int emailId, int attachmentId,
                                                    CancellationToken ct = default);
    }
}
