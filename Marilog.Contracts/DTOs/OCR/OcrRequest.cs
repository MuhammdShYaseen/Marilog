

namespace Marilog.Contracts.DTOs.OCR
{
    public sealed record OcrRequest 
    {
        public Guid DocumentId { get; set; }
        public string FilePath { get; set; } = string.Empty;
    }
}
