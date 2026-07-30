namespace Marilog.Infrastructure.OCR.Models
{
    public sealed record OcrDocumentResult
    (
        string InputPath,
        string OutputPath,
        int TotalPages,
        int ProcessedPages,
        IReadOnlyList<OcrPageResult> Pages,
        TimeSpan Duration
    );
}
