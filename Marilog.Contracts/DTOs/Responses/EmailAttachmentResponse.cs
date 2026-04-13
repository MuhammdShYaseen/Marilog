namespace Marilog.Contracts.DTOs.Responses
{
    public class EmailAttachmentResponse
    {
        public int EmailId { get; private set; }
        public string FileName { get; private set; } = null!;
        public string FilePath { get; private set; } = null!;
        public long FileSizeBytes { get; private set; }
    }
}
