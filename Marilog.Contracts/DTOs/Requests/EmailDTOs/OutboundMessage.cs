

namespace Marilog.Contracts.DTOs.Requests.EmailDTOs
{
    public class OutboundMessage
    {
        public string Subject { get; set; } = null!;
        public string Body { get; set; } = null!;
        public List<string> ToAddresses { get; set; } = new();
        public List<string> CcAddresses { get; set; } = new();
        public List<OutboundAttachment> Attachments { get; set; } = new();
    }
}
