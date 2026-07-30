namespace Marilog.Infrastructure.Models.OCR
{
    public sealed record OcrBoundingBox(
    int X1, int Y1,
    int X2, int Y2
)
    {
        public int Width => X2 - X1;
        public int Height => Y2 - Y1;
    }
}
