namespace Marilog.Application.Models.DocumentExtraction
{
    public sealed class TextLine
    {
        public List<PdfWord> Words { get; } = [];
        public double Top { get; private set; } = double.MinValue;
        public double Bottom { get; private set; } = double.MaxValue;
        public sealed record LineSegment(string Text, double Left, double Right);
        public double CenterY => (Top + Bottom) / 2;
        public string Text => ExtractionParsers.CollapseSpacedLetters(string.Join(' ', Words.Select(w => w.Text)));

        public bool Accepts(PdfWord w) =>
            (w.CenterY <= Top && w.CenterY >= Bottom) ||
            (CenterY <= w.Top && CenterY >= w.Bottom);

        public void Add(PdfWord w)
        {
            Words.Add(w);
            Top = Math.Max(Top, w.Top);
            Bottom = Math.Min(Bottom, w.Bottom);
        }

        public IReadOnlyList<LineSegment> SegmentSpans(double gapFactor = 1.0)
        {
            var result = new List<LineSegment>();
            var start = 0;

            for (var i = 1; i <= Words.Count; i++)
            {
                var split = i == Words.Count;
                if (!split)
                {
                    var prev = Words[i - 1];
                    var height = Math.Max(prev.Top - prev.Bottom, Words[i].Top - Words[i].Bottom);
                    split = Words[i].Left - prev.Right > height * gapFactor;
                }

                if (split)
                {
                    var part = Words.GetRange(start, i - start);
                    result.Add(new LineSegment(
                        ExtractionParsers.CollapseSpacedLetters(string.Join(' ', part.Select(w => w.Text))),
                        part[0].Left,
                        part.Max(w => w.Right)));
                    start = i;
                }
            }

            return result;
        }

        public IReadOnlyList<string> Segments(double gapFactor = 1.0) =>
            SegmentSpans(gapFactor).Select(s => s.Text).ToList();
    }
}