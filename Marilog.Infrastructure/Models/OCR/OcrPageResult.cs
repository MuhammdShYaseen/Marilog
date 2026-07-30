namespace Marilog.Infrastructure.Models.OCR
{
    public sealed record OcrPageResult(int PageNumber, IReadOnlyList<OcrWord> Words, int PageWidthPx, int PageHeightPx
);
}
