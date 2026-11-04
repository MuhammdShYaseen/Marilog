namespace Marilog.Application.Models.DocumentExtraction
{
    public sealed class TextLine
    {
        public List<PdfWord> Words { get; } = [];
        public double Top { get; private set; } = double.MinValue;
        public double Bottom { get; private set; } = double.MaxValue;
        public double CenterY => (Top + Bottom) / 2;
        public string Text => string.Join(' ', Words.Select(w => w.Text));

        public bool Accepts(PdfWord w) =>
            (w.CenterY <= Top && w.CenterY >= Bottom) ||
            (CenterY <= w.Top && CenterY >= w.Bottom);

        public void Add(PdfWord w)
        {
            Words.Add(w);
            Top = Math.Max(Top, w.Top);
            Bottom = Math.Min(Bottom, w.Bottom);
        }

        public IReadOnlyList<string> Segments(double gapFactor = 1.0)
        {
            var segments = new List<string>();
            var current = new List<string>();

            for (var i = 0; i < Words.Count; i++)
            {
                if (i > 0)
                {
                    var prev = Words[i - 1];
                    var height = Math.Max(prev.Top - prev.Bottom, Words[i].Top - Words[i].Bottom);
                    if (Words[i].Left - prev.Right > height * gapFactor)
                    {
                        segments.Add(string.Join(' ', current));
                        current.Clear();
                    }
                }
                current.Add(Words[i].Text);
            }

            if (current.Count > 0) segments.Add(string.Join(' ', current));
            return segments;
        }
    }
}