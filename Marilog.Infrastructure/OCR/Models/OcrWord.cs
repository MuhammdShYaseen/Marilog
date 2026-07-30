

namespace Marilog.Infrastructure.OCR.Models
{
    public sealed record OcrWord(string Text, float Confidence, OcrBoundingBox BoundingBox);
}
