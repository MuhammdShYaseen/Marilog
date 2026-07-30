

namespace Marilog.Infrastructure.OCR.Models
{
    public sealed record OcrPageResult(int PageNumber, IReadOnlyList<OcrWord> Words, int PageWidthPx, int PageHeightPx
);
}
