using System.Linq.Expressions;
using Marilog.Contracts.DTOs.Requests.EmailDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.EmailServices;
using Marilog.Domain.Entities.SystemEntities;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Kernel.Enums;
using Microsoft.EntityFrameworkCore;

namespace Marilog.Application.Services.ApplicationServices.SystemServices
{
    public class EmailService : IEmailService
    {
        private readonly IRepository<Email> _repo;
        private readonly IEmailAccountService _accountService;
        private readonly IEmailProviderClientFactory _clientFactory;
        public EmailService(IRepository<Email> repo, IEmailAccountService accountService, IEmailProviderClientFactory clientFactory)
        {
            _repo = repo;
            _accountService = accountService;
            _clientFactory = clientFactory;

        }

        // ── Mapping ───────────────────────────────────────────────────────────────

        private static readonly Expression<Func<Email, EmailResponse>> ToResponse = email => new EmailResponse
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
                    DisplayName = p.DisplayName,
                    EmailAddress = p.EmailAddress,
                    ParticipantType = p.ParticipantType,
                    Role = p.Role
                })
                .ToList(),
            AccountID = email.AccountID,
            AccountNav = new EmailAccountResponse 
            { 
                EmailAddress = email.Account != null ? email.Account.EmailAddress : null,
                DisplayName = email.Account != null ? email.Account.DisplayName : null,
                LastSyncedAt = email.Account != null ? email.Account.LastSyncedAt : null,
                ProviderType = email.Account != null ? email.Account.ProviderType : 0,
            }
            
        };

        // Used for in-memory (non-queryable) entities, e.g. right after Create.
        private static readonly Func<Email, EmailResponse> ToResponseCompiled = ToResponse.Compile();

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<EmailResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(ToResponse)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<EmailResponse?> GetFullAsync(int id, CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(ToResponse)
                .FirstOrDefaultAsync(ct);
        }
        public async Task<IReadOnlyList<EmailResponse>> GetUnlinkedAsync(CancellationToken ct = default)
        {
            // Powers the Triage screen — inbound emails not yet linked to any entity.
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.EntityType == EntityType.NON && x.EntityId == null)
                .OrderByDescending(x => x.CreatedAt)
                .Select(ToResponse)
                .ToListAsync(ct);
        }
        public async Task<IReadOnlyList<EmailResponse>> GetByEntityAsync(EntityType entityType,
            int entityId, CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.EntityType == entityType &&
                            x.EntityId == entityId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(ToResponse)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<EmailResponse>> GetByStatusAsync(EmailStatus status,
            CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.Status == status)
                .OrderByDescending(x => x.CreatedAt)
                .Select(ToResponse)
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
                .Select(ToResponse)
                .ToListAsync(ct);
        }

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<EmailResponse> CreateAsync(CreateEmailRequest request, CancellationToken ct = default)
        {
            if (!request.Participants.Any(p => p.Role == ParticipantRole.From))
                throw new InvalidOperationException("Email must have a sender.");
            if (!request.Participants.Any(p => p.Role == ParticipantRole.To))
                throw new InvalidOperationException("Email must have at least one recipient.");

            var email = Email.Create(request.EntityType, request.EntityId, request.AccountID, request.Subject, request.Body, request.Direction);

            foreach (var p in request.Participants)
                email.AddParticipant(p.Role, p.ParticipantType,
                                     p.ParticipantId, p.DisplayName, p.EmailAddress);

            await _repo.AddAsync(email, ct);
            await _repo.SaveChangesAsync(ct);

            return ToResponseCompiled(email);
        }


        /// <summary>
        /// Called by the mail sync background service for every message fetched
        /// from an inbox. Always creates an unlinked email (EntityType.NON /
        /// EntityId null) — linking happens later via RelinkAsync from the
        /// Triage screen. Sender/recipients are recorded as unmatched
        /// participants (ParticipantId null) since we have no reliable way here
        /// to resolve an inbound address to a known Company/Vessel — that
        /// matching, if wanted, is a separate concern (e.g. match against
        /// Company.Email), not this method's job.
        /// Attachment upload to StoredFile is NOT handled here; caller's job.
        /// </summary>
        public async Task<EmailResponse> CreateFromInboundAsync(int accountId, InboundMessage message, CancellationToken ct = default)
        {
            var existingId = await _repo.Query()
                .AsNoTracking()
                .Where(x => x.AccountID == accountId && x.ExternalId == message.ExternalId)
                .Select(x => (int?)x.Id)
                .FirstOrDefaultAsync(ct);

            if (existingId is not null)
                return (await GetByIdAsync(existingId.Value, ct))!; // already synced, avoid duplicate

            var email = Email.CreateInbound(accountId, message.Subject, message.Body,
                message.ExternalId, message.ReceivedAt);

            email.AddParticipant(ParticipantRole.From, ParticipantType.Company,
                participantId: null, displayName: message.FromDisplayName, emailAddress: message.FromAddress);

            foreach (var to in message.ToAddresses)
                email.AddParticipant(ParticipantRole.To, ParticipantType.Company,
                    participantId: null, emailAddress: to);

            foreach (var cc in message.CcAddresses)
                email.AddParticipant(ParticipantRole.Cc, ParticipantType.Company,
                    participantId: null, emailAddress: cc);

            await _repo.AddAsync(email, ct);
            await _repo.SaveChangesAsync(ct);

            return ToResponseCompiled(email);
        }
        /// <summary>
        /// Sends an existing Draft email through its EmailAccount's provider,
        /// then marks it Sent (or Failed if the provider call throws).
        /// Note: currently sends Subject/Body only — StoredFile attachments
        /// linked to this email (EntityType.Email, EntityId = emailId) are not
        /// yet fetched/attached here; that needs IStoredFileService's download
        /// signature, which isn't wired up yet.
        /// </summary>
        public async Task<EmailResponse> SendEmailAsync(int emailId, CancellationToken ct = default)
        {
            var email = await GetWithParticipantsOrThrowAsync(emailId, ct);

            var account = await _accountService.GetByIdAsync(email.AccountID, ct)
                ?? throw new KeyNotFoundException($"EmailAccount {email.AccountID} not found.");

            var toAddresses = email.Recipients
                .Select(p => p.EmailAddress)
                .Where(a => !string.IsNullOrWhiteSpace(a))
                .Select(a => a!)
                .ToList();

            var ccAddresses = email.CcList
                .Select(p => p.EmailAddress)
                .Where(a => !string.IsNullOrWhiteSpace(a))
                .Select(a => a!)
                .ToList();

            if (toAddresses.Count == 0)
                throw new InvalidOperationException(
                    "Cannot send: no recipient has a resolvable EmailAddress.");

            var outbound = new OutboundMessage
            {
                Subject = email.Subject,
                Body = email.Body,
                ToAddresses = toAddresses,
                CcAddresses = ccAddresses
            };

            var config = await _accountService.GetDecryptedConfigAsync(email.AccountID, ct);
            var client = _clientFactory.GetClient(account.ProviderType);

            try
            {
                if(account.EmailAddress != null)
                await client.SendAsync(config, account.EmailAddress, account.DisplayName, outbound, ct);
            }
            catch
            {
                email.MarkAsFailed();
                _repo.Update(email);
                await _repo.SaveChangesAsync(ct);
                throw;
            }

            email.MarkAsSent(DateTime.UtcNow);
            _repo.Update(email);
            await _repo.SaveChangesAsync(ct);

            return ToResponseCompiled(email);
        }
        public async Task<EmailResponse> UpsertAsync(int emailId, EntityType entityType,
            int entityId, CancellationToken ct = default)
        {
            var email = await _repo.Query()
                .Include(x => x.Participants)
                .FirstOrDefaultAsync(x => x.Id == emailId, ct)
                ?? throw new KeyNotFoundException(
                    $"Email {emailId} not found. Upsert only re-links an existing email " +
                    "to a different entity — use CreateAsync to create a new one.");

            email.Upsert(entityType, entityId);

            _repo.Update(email);
            await _repo.SaveChangesAsync(ct);

            return ToResponseCompiled(email);
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
                //CompanyId = participant.CompanyId,
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

    }
}