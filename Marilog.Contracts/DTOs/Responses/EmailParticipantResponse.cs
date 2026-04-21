using Marilog.Kernel.Enums;


namespace Marilog.Contracts.DTOs.Responses
{
    public class EmailParticipantResponse 
    {
        public int Id { get;  set; }

        public int EmailId { get;  set; }
        //public Email Email { get; private set; } = null!;
        public ParticipantRole Role { get;  set; }
        public ParticipantType ParticipantType { get;  set; }
        public int ParticipantId { get;  set; }  // CompanyId or VesselId

        // ── Navigation (resolved at query time) ───────────────────────────────────
        public int CompanyId { get;  set; }



        // ── Fallback / override display ───────────────────────────────────────────
        public string? DisplayName { get;  set; }  // override Company/Vessel name if needed
        public string? EmailAddress { get;  set; }  // override or fallback address
    };
}
