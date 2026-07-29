using Marilog.Domain.Common;
using Marilog.Kernel.Enums;
using System.ComponentModel.DataAnnotations.Schema;


namespace Marilog.Domain.Entities.SystemEntities
{
   

    /// <summary>
    /// Standalone audit aggregate — logs every email exchanged during
    /// the procurement lifecycle. Linked to any entity via (EntityType, EntityId).
    /// Participants are Companies or Vessels — resolved via EmailParticipant.
    /// </summary>
    public class Email : Entity
    {

        // ── Polymorphic reference to owning entity ────────────────────────────────
        public EntityType EntityType { get; private set; } = 0;  // "Document" | "SwiftTransfer" | "Voyage"
        public int? EntityId { get; private set; }

        // ── Content ───────────────────────────────────────────────────────────────
        public string Subject { get; private set; } = null!;
        public string Body { get; private set; } = null!;

        // ── Meta ──────────────────────────────────────────────────────────────────
        public EmailDirection Direction { get; private set; }
        public EmailStatus Status { get; private set; } = EmailStatus.Draft;
        public DateTime? SentAt { get; private set; }
        public string? ExternalId { get; private set; }  // Message-ID from mail server

        private readonly List<EmailParticipant> _participants = new();
        public IReadOnlyCollection<EmailParticipant> Participants => _participants.AsReadOnly();


        //----Email Account---------------------------------------------------------
        public int AccountID { get; private set; }
        public EmailAccount? Account { get; private set; }

        // ── Computed shortcuts ────────────────────────────────────────────────────
        [NotMapped]
        public EmailParticipant? Sender => _participants.FirstOrDefault(p => p.Role == ParticipantRole.From);
        [NotMapped]
        public IEnumerable<EmailParticipant> Recipients => _participants.Where(p => p.Role == ParticipantRole.To);
        [NotMapped]
        public IEnumerable<EmailParticipant> CcList => _participants.Where(p => p.Role == ParticipantRole.Cc);

        private Email() { }

        // ── Factory ───────────────────────────────────────────────────────────────
        public static Email Create(EntityType entityType, int entityId, int accountId, string subject, string body, EmailDirection direction = EmailDirection.Outbound)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(subject);
            ArgumentException.ThrowIfNullOrWhiteSpace(body);

            return new Email
            {
                EntityType = entityType,
                EntityId = entityId,
                AccountID = accountId,
                Subject = subject,
                Body = body,
                Direction = direction,
                Status = EmailStatus.Draft
            };
        }

        public static Email CreateInbound(int accountId, string subject, string body,
            string externalId, DateTime receivedAt)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(subject);
            ArgumentException.ThrowIfNullOrWhiteSpace(body);

            return new Email
            {
                AccountID = accountId,
                EntityType = EntityType.NON,
                EntityId = null,
                Subject = subject,
                Body = body,
                Direction = EmailDirection.Inbound,
                Status = EmailStatus.Received,
                ExternalId = externalId,
                SentAt = receivedAt
            };
        }

        public static Email CreateFromSentSync(int accountId, string subject, string body,
           string externalId, DateTime sentAt)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(subject);
            ArgumentException.ThrowIfNullOrWhiteSpace(body);

            return new Email
            {
                AccountID = accountId,
                EntityType = EntityType.NON,
                EntityId = null,
                Subject = subject,
                Body = body,
                Direction = EmailDirection.Outbound,
                Status = EmailStatus.Sent,
                ExternalId = externalId,
                SentAt = sentAt
            };
        }
        public void Relink(EntityType entityType, int entityId)
        {
            EntityType = entityType;
            EntityId = entityId;
        }
        // ── Participants ──────────────────────────────────────────────────────────
        public EmailParticipant AddParticipant(
            ParticipantRole role,
            ParticipantType participantType,
            int? participantId,
            string? displayName = null,
            string? emailAddress = null)
        {
            if (role == ParticipantRole.From && _participants.Any(p => p.Role == ParticipantRole.From))
                throw new InvalidOperationException("Email can only have one sender.");

            if (participantId <= 0)
                throw new ArgumentException("Invalid ParticipantId.");

            var participant = EmailParticipant.Create(
                Id, role, participantType, participantId, displayName, emailAddress);

            _participants.Add(participant);
            Touch();
            return participant;
        }

        public void RemoveParticipant(int participantId)
        {
            var participant = _participants.FirstOrDefault(p => p.Id == participantId)
                ?? throw new InvalidOperationException($"Participant {participantId} not found.");

            if (participant.Role == ParticipantRole.From)
                throw new InvalidOperationException("Cannot remove the sender.");

            _participants.Remove(participant);
            Touch();
        }

        // ── Validation guard before sending ──────────────────────────────────────
        private void EnsureReadyToSend()
        {
            if (!_participants.Any(p => p.Role == ParticipantRole.From))
                throw new InvalidOperationException("Email must have a sender before sending.");
            if (!_participants.Any(p => p.Role == ParticipantRole.To))
                throw new InvalidOperationException("Email must have at least one recipient.");
        }

        // ── Status transitions ────────────────────────────────────────────────────
        public void MarkAsSent(DateTime sentAt, string? externalId = null)
        {
            EnsureReadyToSend();
            if (Status == EmailStatus.Sent)
                throw new InvalidOperationException("Email is already marked as sent.");

            Status = EmailStatus.Sent;
            SentAt = sentAt;
            ExternalId = externalId;
            Touch();
        }

        public void MarkAsReceived()
        {
            Status = EmailStatus.Received;
            Touch();
        }

        public void MarkAsFailed()
        {
            Status = EmailStatus.Failed;
            Touch();
        }

        public void Retry()
        {
            if (Status != EmailStatus.Failed)
                throw new InvalidOperationException("Only failed emails can be retried.");
            Status = EmailStatus.Draft;
            Touch();
        }

        
    }
}
