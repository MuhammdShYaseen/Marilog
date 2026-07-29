
namespace Marilog.Contracts.DTOs.Requests.EmailDTOs
{
    public class InboundMessage
    {
        public string ExternalId { get; set; } = null!; // Message-ID header, used for de-dup
        public string Subject { get; set; } = null!;
        public string Body { get; set; } = null!;
        public DateTime ReceivedAt { get; set; }

        public string FromAddress { get; set; } = null!;
        public string? FromDisplayName { get; set; }

        public List<string> ToAddresses { get; set; } = new();
        public List<string> CcAddresses { get; set; } = new();

        public List<InboundAttachment> Attachments { get; set; } = new();
    }
}
