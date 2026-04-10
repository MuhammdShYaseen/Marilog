using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Enums;

namespace Marilog.Contracts.DTOs.Requests.DocumentDTOs
{
    public class LogEmailRequest
    {
        public string Subject { get; set; } = default!;
        public string Body { get; set; } = default!;
        public IReadOnlyList<EmailParticipantData> Participants { get; set; } = new List<EmailParticipantData>();
        public EmailDirection Direction { get; set; }
    }
}
