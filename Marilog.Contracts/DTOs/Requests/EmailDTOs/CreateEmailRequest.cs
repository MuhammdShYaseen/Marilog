using Marilog.Contracts.DTOs.Responses;
using Marilog.Kernel.Enums;

namespace Marilog.Contracts.DTOs.Requests.EmailDTOs
{
    public class CreateEmailRequest
    {
        public EntityType EntityType { get; set; } = default!;
        public int EntityId { get; set; }

        public int AccountID { get; set; }
        public string Subject { get; set; } = default!;
        public string Body { get; set; } = default!;
        public EmailDirection Direction { get; set; }

        public IReadOnlyList<EmailParticipantResponse> Participants { get; set; }
            = new List<EmailParticipantResponse>();
      
    }
}
