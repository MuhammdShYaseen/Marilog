namespace Marilog.Infrastructure.Models.OCR
{
    public sealed record OcrWord(string Text, float Confidence, OcrBoundingBox BoundingBox);
}
