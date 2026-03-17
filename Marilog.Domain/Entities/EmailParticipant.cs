using Marilog.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marilog.Domain.Entities
{
    /// <summary>
    /// Owned entity inside Email aggregate.
    /// Represents a From / To / Cc participant — either a Company or a Vessel.
    /// EmailAddress is optional: used as fallback when the entity has no email on record.
    /// </summary>
    public class EmailParticipant
    {
        public int Id { get; private set; }
        
        public int EmailId { get; private set; }
        //public Email Email { get; private set; } = null!;
        public ParticipantRole Role { get; private set; }
        public ParticipantType ParticipantType { get; private set; }
        public int ParticipantId { get; private set; }  // CompanyId or VesselId

        // ── Navigation (resolved at query time) ───────────────────────────────────
        public Company Company { get; private set; } = null!;


        public int CompanyId {  get; private set; }

       

        // ── Fallback / override display ───────────────────────────────────────────
        public string? DisplayName { get; private set; }  // override Company/Vessel name if needed
        public string? EmailAddress { get; private set; }  // override or fallback address

        private EmailParticipant() { }

        internal static EmailParticipant Create(
            int emailId,
            ParticipantRole role,
            ParticipantType participantType,
            int participantId,
            string? displayName = null,
            string? emailAddress = null)
        {
            return new EmailParticipant
            {
                EmailId = emailId,
                Role = role,
                ParticipantType = participantType,
                ParticipantId = participantId,
                DisplayName = displayName,
                EmailAddress = emailAddress
            };
        }
    }
}