

namespace Marilog.Contracts.DTOs.Requests.EmailDTOs
{
    public class OutboundAttachment
    {
        public string FileName { get; set; } = null!;
        public string ContentType { get; set; } = null!;
        public byte[] Content { get; set; } = null!;
    }
}
