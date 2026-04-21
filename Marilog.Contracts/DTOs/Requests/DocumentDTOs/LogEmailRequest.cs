using Marilog.Contracts.DTOs.Responses;
using Marilog.Kernel.Enums;


namespace Marilog.Contracts.DTOs.Requests.DocumentDTOs
{
    public class LogEmailRequest
    {
        public string Subject { get; set; } = default!;
        public string Body { get; set; } = default!;
        public IReadOnlyList<EmailParticipantResponse> Participants { get; set; } = new List<EmailParticipantResponse>();
        public EmailDirection Direction { get; set; }
    }
}
