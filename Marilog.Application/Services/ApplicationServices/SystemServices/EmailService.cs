using Microsoft.EntityFrameworkCore;
using Marilog.Domain.Events;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Domain.Entities.SystemEntities;
using Marilog.Kernel.Enums;
using Marilog.Contracts.Interfaces.Services;
using Marilog.Contracts.DTOs.Responses;

namespace Marilog.Application.Services.ApplicationServices.SystemServices
{
    public class EmailService : IEmailService
    {
        private readonly IRepository<Email> _repo;

        public EmailService(IRepository<Email> repo) => _repo = repo;

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<EmailResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(email => new EmailResponse
                {
                    Id = email.Id,

                    Direction = email.Direction,
                    SentAt = email.SentAt,
                    Status = email.Status,
                    Subject = email.Subject,
                    Body = email.Body,
                    EntityId = email.EntityId,
                    EntityType = email.EntityType,
                    ExternalId = email.ExternalId,

                    Attachments = email.Attachments
                .Select(a => new EmailAttachmentResponse
                {
                    EmailId = a.Id,
                    FileName = a.FileName,
                    FilePath = a.FilePath,
                    FileSizeBytes = a.FileSizeBytes
                })
                .ToList(),

                Participants = email.Participants
                .Select(p => new EmailParticipantResponse
                {
                    CompanyId = p.CompanyId,
                    DisplayName = p.DisplayName,
                    EmailAddress = p.EmailAddress,
                    EmailId = p.EmailId,
                    ParticipantId = p.ParticipantId,
                    Id = p.Id,
                    ParticipantType = p.ParticipantType,
                    Role = p.Role
                }).ToList()
        }).FirstOrDefaultAsync(ct);
        } 

        public async Task<EmailResponse?> GetFullAsync(int id, CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(email => new EmailResponse
                {
                    Id = email.Id,

                    Direction = email.Direction,
                    SentAt = email.SentAt,
                    Status = email.Status,
                    Subject = email.Subject,
                    Body = email.Body,
                    EntityId = email.EntityId,
                    EntityType = email.EntityType,
                    ExternalId = email.ExternalId,

                    Attachments = email.Attachments
                        .Select(a => new EmailAttachmentResponse
                        {
                            EmailId = a.Id,
                            FileName = a.FileName,
                            FilePath = a.FilePath,
                            FileSizeBytes = a.FileSizeBytes
                        })
                        .ToList(),

                    Participants = email.Participants
                        .Select(p => new EmailParticipantResponse
                        {
                            Id = p.Id,
                            EmailId = p.EmailId,
                            ParticipantId = p.ParticipantId,
                            CompanyId = p.CompanyId,
                            DisplayName = p.DisplayName,
                            EmailAddress = p.EmailAddress,
                            ParticipantType = p.ParticipantType,
                            Role = p.Role
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync(ct);
        }

        public async Task<IReadOnlyList<EmailResponse>> GetByEntityAsync(string entityType,
            int entityId, CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.EntityType == entityType &&
                            x.EntityId == entityId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(email => new EmailResponse
                {
                    Id = email.Id,

                    Direction = email.Direction,
                    SentAt = email.SentAt,
                    Status = email.Status,
                    Subject = email.Subject,
                    Body = email.Body,
                    EntityId = email.EntityId,
                    EntityType = email.EntityType,
                    ExternalId = email.ExternalId,

                    Participants = email.Participants
                        .Select(p => new EmailParticipantResponse
                        {
                            Id = p.Id,
                            EmailId = p.EmailId,
                            ParticipantId = p.ParticipantId,
                            CompanyId = p.CompanyId,
                            DisplayName = p.DisplayName,
                            EmailAddress = p.EmailAddress,
                            ParticipantType = p.ParticipantType,
                            Role = p.Role
                        })
                        .ToList(),

                    Attachments = email.Attachments
                        .Select(a => new EmailAttachmentResponse
                        {
                            EmailId = a.Id,
                            FileName = a.FileName,
                            FilePath = a.FilePath,
                            FileSizeBytes = a.FileSizeBytes
                        })
                        .ToList()
                })
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<EmailResponse>> GetByStatusAsync(EmailStatus status,
            CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.Status == status)
                .OrderByDescending(x => x.CreatedAt)
                .Select(email => new EmailResponse
                {
                    Id = email.Id,

                    Direction = email.Direction,
                    SentAt = email.SentAt,
                    Status = email.Status,
                    Subject = email.Subject,
                    Body = email.Body,
                    EntityId = email.EntityId,
                    EntityType = email.EntityType,
                    ExternalId = email.ExternalId,

                    Participants = email.Participants
                        .Select(p => new EmailParticipantResponse
                        {
                            Id = p.Id,
                            EmailId = p.EmailId,
                            ParticipantId = p.ParticipantId,
                            CompanyId = p.CompanyId,
                            DisplayName = p.DisplayName,
                            EmailAddress = p.EmailAddress,
                            ParticipantType = p.ParticipantType,
                            Role = p.Role
                        })
                        .ToList(),

                    Attachments = email.Attachments
                        .Select(a => new EmailAttachmentResponse
                        {
                            EmailId = a.Id,
                            FileName = a.FileName,
                            FilePath = a.FilePath,
                            FileSizeBytes = a.FileSizeBytes
                        })
                        .ToList()
                })
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<EmailResponse>> GetByParticipantAsync(
            ParticipantType participantType, int participantId,
            CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.Participants.Any(
                    p => p.ParticipantType == participantType &&
                         p.ParticipantId == participantId))
                .OrderByDescending(x => x.CreatedAt)
                .Select(email => new EmailResponse
                {
                    Id = email.Id,

                    Direction = email.Direction,
                    SentAt = email.SentAt,
                    Status = email.Status,
                    Subject = email.Subject,
                    Body = email.Body,
                    EntityId = email.EntityId,
                    EntityType = email.EntityType,
                    ExternalId = email.ExternalId,

                    Participants = email.Participants
                        .Select(p => new EmailParticipantResponse
                        {
                            Id = p.Id,
                            EmailId = p.EmailId,
                            ParticipantId = p.ParticipantId,
                            CompanyId = p.CompanyId,
                            DisplayName = p.DisplayName,
                            EmailAddress = p.EmailAddress,
                            ParticipantType = p.ParticipantType,
                            Role = p.Role
                        })
                        .ToList(),

                    Attachments = email.Attachments
                        .Select(a => new EmailAttachmentResponse
                        {
                            EmailId = a.Id,
                            FileName = a.FileName,
                            FilePath = a.FilePath,
                            FileSizeBytes = a.FileSizeBytes
                        })
                        .ToList()
                })
                .ToListAsync(ct);
        }

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<EmailResponse> CreateAsync(string entityType, int entityId,
            string subject, string body, EmailDirection direction,
            IReadOnlyList<EmailParticipantResponse> participants,
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
            return new EmailResponse
            {
                Id = email.Id,

                Direction = email.Direction,
                SentAt = email.SentAt,
                Status = email.Status,
                Subject = email.Subject,
                Body = email.Body,
                EntityId = email.EntityId,
                EntityType = email.EntityType,
                ExternalId = email.ExternalId,

                Participants = email.Participants
           .Select(p => new EmailParticipantResponse
           {
               Id = p.Id,
               EmailId = p.EmailId,
               ParticipantId = p.ParticipantId,
               CompanyId = p.CompanyId,
               DisplayName = p.DisplayName,
               EmailAddress = p.EmailAddress,
               ParticipantType = p.ParticipantType,
               Role = p.Role
           })
           .ToList(),

                Attachments = email.Attachments
           .Select(a => new EmailAttachmentResponse
           {
               EmailId = a.Id,
               FileName = a.FileName,
               FilePath = a.FilePath,
               FileSizeBytes = a.FileSizeBytes
           })
           .ToList()
            };
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

        public async Task<EmailParticipantResponse> AddParticipantAsync(int emailId,
            ParticipantRole role, ParticipantType participantType, int participantId,
            string? displayName = null, string? emailAddress = null,
            CancellationToken ct = default)
        {
            var email = await GetWithParticipantsOrThrowAsync(emailId, ct);
            var participant = email.AddParticipant(role, participantType,
                                                   participantId, displayName, emailAddress);
            _repo.Update(email);
            await _repo.SaveChangesAsync(ct);
            return new EmailParticipantResponse
            {
                Id = participant.Id,
                EmailId = participant.EmailId,
                ParticipantId = participant.ParticipantId,
                CompanyId = participant.CompanyId,
                DisplayName = participant.DisplayName,
                EmailAddress = participant.EmailAddress,
                ParticipantType = participant.ParticipantType,
                Role = participant.Role
            };
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

        public async Task<EmailAttachmentResponse> AddAttachmentAsync(int emailId, string fileName,
            string filePath, long fileSizeBytes, CancellationToken ct = default)
        {
            var email = await GetWithAttachmentsOrThrowAsync(emailId, ct);
            var attachment = email.AddAttachment(fileName, filePath, fileSizeBytes);
            _repo.Update(email);
            await _repo.SaveChangesAsync(ct);
            return new EmailAttachmentResponse
            {
                EmailId = attachment.Id,
                FileName = attachment.FileName,
                FilePath = attachment.FilePath,
                FileSizeBytes = attachment.FileSizeBytes
            };
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
