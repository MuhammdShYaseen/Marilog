namespace Marilog.Contracts.DTOs.Responses
{
    public class EmailAttachmentResponse
    {
        public int EmailId { get;  set; }
        public string FileName { get;  set; } = null!;
        public string FilePath { get;  set; } = null!;
        public long FileSizeBytes { get;  set; }
    }
}
