namespace Marilog.Contracts.DTOs.Requests.EmailDTOs
{
    public class AddAttachmentRequest
    {
        public string FileName { get; set; } = default!;
        public string FilePath { get; set; } = default!;
        public long FileSizeBytes { get; set; }
    }
}
