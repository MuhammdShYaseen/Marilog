namespace Marilog.Infrastructure.Models.OCR
{
    public sealed record OcrProgress(int CurrentPage, int TotalPages, string Status);
}
