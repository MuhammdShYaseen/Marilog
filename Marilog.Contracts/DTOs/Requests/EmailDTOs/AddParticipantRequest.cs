
using Marilog.Contracts.Enums;

namespace Marilog.Contracts.DTOs.Requests.EmailDTOs
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
