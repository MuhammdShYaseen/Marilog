

namespace Marilog.Infrastructure.OCR.Models
{
    public sealed record OcrProgress(int CurrentPage, int TotalPages, string Status);
}
