

namespace Marilog.Application.Models.DocumentExtraction
{
    public static class LineGrouper
    {
        public static IReadOnlyList<TextLine> Group(IReadOnlyList<PdfWord> words)
        {
            var lines = new List<TextLine>();

            foreach (var word in words.OrderByDescending(w => w.Top))
            {
                var line = lines.FirstOrDefault(l => l.Accepts(word));
                if (line is null)
                {
                    line = new TextLine();
                    lines.Add(line);
                }
                line.Add(word);
            }

            foreach (var line in lines)
                line.Words.Sort((a, b) => a.Left.CompareTo(b.Left));

            return lines.OrderByDescending(l => l.Top).ToList();
        }
    }
}