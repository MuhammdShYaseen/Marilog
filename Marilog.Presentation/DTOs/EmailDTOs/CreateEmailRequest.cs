using Marilog.Domain.Entities.SystemEntities;
using Marilog.Domain.Events;

namespace Marilog.Presentation.DTOs.EmailDTOs
{
    public class CreateEmailRequest
    {
        public string EntityType { get; set; } = default!;
        public int EntityId { get; set; }

        public string Subject { get; set; } = default!;
        public string Body { get; set; } = default!;
        public EmailDirection Direction { get; set; }

        public IReadOnlyList<EmailParticipantData> Participants { get; set; }
            = new List<EmailParticipantData>();
    }
}
