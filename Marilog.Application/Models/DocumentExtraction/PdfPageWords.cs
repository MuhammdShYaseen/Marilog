namespace Marilog.Application.Models.DocumentExtraction
{
    public sealed record PdfWord(string Text, double Left, double Right, double Top, double Bottom)
    {
        public double CenterY => (Top + Bottom) / 2;
    }
    public sealed record PdfPageWords(int Number, double Width, double Height, IReadOnlyList<PdfWord> Words)
    {
        // A scanned page has no (or almost no) selectable text.
        public bool HasTextLayer => Words.Count >= 20;
    }
}
