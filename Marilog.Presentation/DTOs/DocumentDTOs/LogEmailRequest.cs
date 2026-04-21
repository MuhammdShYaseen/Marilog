using Marilog.Kernel.Enums;
using Marilog.Domain.Events;

namespace Marilog.Presentation.DTOs.DocumentDTOs
{
    public class LogEmailRequest
    {
        public string Subject { get; set; } = default!;
        public string Body { get; set; } = default!;
        public IReadOnlyList<EmailParticipantData> Participants { get; set; } = new List<EmailParticipantData>();
        public EmailDirection Direction { get; set; }
    }
}
