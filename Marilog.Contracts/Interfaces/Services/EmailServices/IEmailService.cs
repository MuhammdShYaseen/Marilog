using Marilog.Contracts.DTOs.Requests.EmailDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Kernel.Enums;


namespace Marilog.Contracts.Interfaces.Services.EmailServices
{
    public interface IEmailService
    {
        // ── Queries ───────────────────────────────────────────────────────────────
        Task<EmailResponse?>               GetByIdAsync(int id, CancellationToken ct = default);
        Task<EmailResponse?>               GetFullAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<EmailResponse>> GetUnlinkedAsync(CancellationToken ct = default);
        Task<IReadOnlyList<EmailResponse>> GetByEntityAsync(EntityType entityType, int entityId, CancellationToken ct = default);
        Task<IReadOnlyList<EmailResponse>> GetByStatusAsync(EmailStatus status, CancellationToken ct = default);
        Task<IReadOnlyList<EmailResponse>> GetByParticipantAsync(ParticipantType participantType, int participantId, CancellationToken ct = default);

        // ── Commands ─────────────────────────────────────────────────────────────
        Task<EmailResponse> CreateAsync(CreateEmailRequest request, CancellationToken ct = default);
        Task<EmailResponse> CreateFromInboundAsync(int accountId, InboundMessage message, CancellationToken ct = default);
        Task<EmailResponse> SendEmailAsync(int emailId, CancellationToken ct = default);
        Task<EmailResponse> UpsertAsync(int emailId, EntityType entityType, int entityId, CancellationToken ct = default);
        Task        MarkAsSentAsync(int id, DateTime sentAt, string? externalId = null, CancellationToken ct = default);
        Task        MarkAsReceivedAsync(int id, CancellationToken ct = default);
        Task        MarkAsFailedAsync(int id, CancellationToken ct = default);
        Task        RetryAsync(int id, CancellationToken ct = default);
        Task        DeleteAsync(int id, CancellationToken ct = default);

        // ── Participants ──────────────────────────────────────────────────────────
        Task<EmailParticipantResponse> AddParticipantAsync(int emailId, ParticipantRole role,
                                                   ParticipantType participantType,
                                                   int participantId,
                                                   string? displayName = null,
                                                   string? emailAddress = null,
                                                   CancellationToken ct = default);
        Task                   RemoveParticipantAsync(int emailId, int participantId,
                                                      CancellationToken ct = default);

    }
}
