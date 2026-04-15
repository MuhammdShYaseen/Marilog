using Marilog.Domain.Entities.SystemEntities;

namespace Marilog.Presentation.DTOs.EmailDTOs
{
    public class AddParticipantRequest
    {
        public ParticipantRole Role { get; set; }
        public ParticipantType ParticipantType { get; set; }
        public int ParticipantId { get; set; }
        public string? DisplayName { get; set; }
        public string? EmailAddress { get; set; }
    }
}
