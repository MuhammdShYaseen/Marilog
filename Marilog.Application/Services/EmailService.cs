using Microsoft.EntityFrameworkCore;
using Marilog.Domain.Entities;
using Marilog.Domain.Events;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Domain.Interfaces.Services;

namespace Marilog.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly IRepository<Email> _repo;

        public EmailService(IRepository<Email> repo) => _repo = repo;

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<Email?> GetByIdAsync(int id, CancellationToken ct = default)
            => await _repo.Query()
                          .Include(x => x.Participants)
                          .FirstOrDefaultAsync(x => x.Id == id, ct);

        public async Task<Email?> GetFullAsync(int id, CancellationToken ct = default)
            => await _repo.Query()
                          .Include(x => x.Participants).ThenInclude(x => x.Company)
                          .Include(x => x.Participants).ThenInclude(x => x.Vessel)
                          .Include(x => x.Attachments)
                          .FirstOrDefaultAsync(x => x.Id == id, ct);

        public async Task<IReadOnlyList<Email>> GetByEntityAsync(string entityType,
            int entityId, CancellationToken ct = default)
            => await _repo.Query()
                          .Where(x => x.EntityType == entityType &&
                                      x.EntityId   == entityId)
                          .Include(x => x.Participants)
                          .OrderByDescending(x => x.CreatedAt)
                          .ToListAsync(ct);

        public async Task<IReadOnlyList<Email>> GetByStatusAsync(EmailStatus status,
            CancellationToken ct = default)
            => await _repo.Query()
                          .Where(x => x.Status == status)
                          .Include(x => x.Participants)
                          .OrderByDescending(x => x.CreatedAt)
                          .ToListAsync(ct);

        public async Task<IReadOnlyList<Email>> GetByParticipantAsync(
            ParticipantType participantType, int participantId,
            CancellationToken ct = default)
            => await _repo.Query()
                          .Where(x => x.Participants.Any(
                              p => p.ParticipantType == participantType &&
                                   p.ParticipantId   == participantId))
                          .Include(x => x.Participants)
                          .OrderByDescending(x => x.CreatedAt)
                          .ToListAsync(ct);

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<Email> CreateAsync(string entityType, int entityId,
            string subject, string body, EmailDirection direction,
            IReadOnlyList<EmailParticipantData> participants,
            CancellationToken ct = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(entityType);

            if (!participants.Any(p => p.Role == ParticipantRole.From))
                throw new InvalidOperationException("Email must have a sender.");
            if (!participants.Any(p => p.Role == ParticipantRole.To))
                throw new InvalidOperationException("Email must have at least one recipient.");

            var email = Email.Create(entityType, entityId, subject, body, direction);

            foreach (var p in participants)
                email.AddParticipant(p.Role, p.ParticipantType,
                                     p.ParticipantId, p.DisplayName, p.EmailAddress);

            await _repo.AddAsync(email, ct);
            await _repo.SaveChangesAsync(ct);
            return email;
        }

        public async Task MarkAsSentAsync(int id, DateTime sentAt,
            string? externalId = null, CancellationToken ct = default)
        {
            var email = await GetWithParticipantsOrThrowAsync(id, ct);
            email.MarkAsSent(sentAt, externalId);
            _repo.Update(email);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task MarkAsReceivedAsync(int id, CancellationToken ct = default)
        {
            var email = await GetOrThrowAsync(id, ct);
            email.MarkAsReceived();
            _repo.Update(email);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task MarkAsFailedAsync(int id, CancellationToken ct = default)
        {
            var email = await GetOrThrowAsync(id, ct);
            email.MarkAsFailed();
            _repo.Update(email);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task RetryAsync(int id, CancellationToken ct = default)
        {
            var email = await GetOrThrowAsync(id, ct);
            email.Retry();
            _repo.Update(email);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var email = await GetOrThrowAsync(id, ct);
            if (email.Status == EmailStatus.Sent)
                throw new InvalidOperationException(
                    "Cannot delete a sent email. Deactivate it instead.");
            _repo.HardDelete(email);
            await _repo.SaveChangesAsync(ct);
        }

        // ── Participants ──────────────────────────────────────────────────────────

        public async Task<EmailParticipant> AddParticipantAsync(int emailId,
            ParticipantRole role, ParticipantType participantType, int participantId,
            string? displayName = null, string? emailAddress = null,
            CancellationToken ct = default)
        {
            var email = await GetWithParticipantsOrThrowAsync(emailId, ct);
            var participant = email.AddParticipant(role, participantType,
                                                   participantId, displayName, emailAddress);
            _repo.Update(email);
            await _repo.SaveChangesAsync(ct);
            return participant;
        }

        public async Task RemoveParticipantAsync(int emailId, int participantId,
            CancellationToken ct = default)
        {
            var email = await GetWithParticipantsOrThrowAsync(emailId, ct);
            email.RemoveParticipant(participantId);
            _repo.Update(email);
            await _repo.SaveChangesAsync(ct);
        }

        // ── Attachments ───────────────────────────────────────────────────────────

        public async Task<EmailAttachment> AddAttachmentAsync(int emailId, string fileName,
            string filePath, long fileSizeBytes, CancellationToken ct = default)
        {
            var email = await GetWithAttachmentsOrThrowAsync(emailId, ct);
            var attachment = email.AddAttachment(fileName, filePath, fileSizeBytes);
            _repo.Update(email);
            await _repo.SaveChangesAsync(ct);
            return attachment;
        }

        public async Task RemoveAttachmentAsync(int emailId, int attachmentId,
            CancellationToken ct = default)
        {
            var email = await GetWithAttachmentsOrThrowAsync(emailId, ct);
            email.RemoveAttachment(attachmentId);
            _repo.Update(email);
            await _repo.SaveChangesAsync(ct);
        }

        // ── Private ───────────────────────────────────────────────────────────────

        private async Task<Email> GetOrThrowAsync(int id, CancellationToken ct)
            => await _repo.GetByIdAsync(id, ct)
               ?? throw new KeyNotFoundException($"Email {id} not found.");

        private async Task<Email> GetWithParticipantsOrThrowAsync(int id,
            CancellationToken ct)
            => await _repo.Query()
                          .Include(x => x.Participants)
                          .FirstOrDefaultAsync(x => x.Id == id, ct)
               ?? throw new KeyNotFoundException($"Email {id} not found.");

        private async Task<Email> GetWithAttachmentsOrThrowAsync(int id,
            CancellationToken ct)
            => await _repo.Query()
                          .Include(x => x.Attachments)
                          .FirstOrDefaultAsync(x => x.Id == id, ct)
               ?? throw new KeyNotFoundException($"Email {id} not found.");
    }
}
