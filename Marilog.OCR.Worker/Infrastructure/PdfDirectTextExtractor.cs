using Marilog.OCR.Worker.Abstractions;
using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace Marilog.OCR.Worker.Infrastructure;

public sealed class PdfDirectTextExtractor : IPdfDirectTextExtractor
{
    public string ExtractText(string filePath, CancellationToken cancellationToken = default)
    {
        var sb = new StringBuilder();

        using var document = PdfDocument.Open(filePath);

        foreach (var page in document.GetPages())
        {
            cancellationToken.ThrowIfCancellationRequested();

            List<Word> words;
            try
            {
                words = page.GetWords().ToList();
            }
            catch
            {
                continue; // صفحة بدون طبقة نص قابلة للاستخراج (سكان صرف)
            }

            if (words.Count == 0)
                continue;

            foreach (var line in GroupIntoLines(words))
            {
                cancellationToken.ThrowIfCancellationRequested();
                sb.AppendLine(BuildLineText(line));
            }

            sb.AppendLine();
        }

        return sb.ToString().TrimEnd();
    }

    // يجمع الكلمات المتقاربة عمودياً (نفس السطر) بناء على تداخل الصناديق الحقيقية
    private static List<List<Word>> GroupIntoLines(List<Word> words)
    {
        var sorted = words.OrderByDescending(w => w.BoundingBox.Top).ToList();
        var lines = new List<List<Word>>();

        foreach (var word in sorted)
        {
            var line = lines.FirstOrDefault(l => VerticallyOverlaps(l[0], word));

            if (line != null)
                line.Add(word);
            else
                lines.Add([word]);
        }

        return lines;
    }

    private static bool VerticallyOverlaps(Word a, Word b)
    {
        var aMid = (a.BoundingBox.Top + a.BoundingBox.Bottom) / 2;
        var bMid = (b.BoundingBox.Top + b.BoundingBox.Bottom) / 2;
        var tolerance = Math.Max(a.BoundingBox.Height, b.BoundingBox.Height) * 0.5;
        return Math.Abs(aMid - bMid) <= tolerance;
    }

    // يبني نص السطر بترتيب القراءة الصحيح حسب اتجاهه الفعلي (RTL أو LTR)
    private static string BuildLineText(List<Word> line)
    {
        var isRtl = line.Any(w => w.Text.Any(IsStrongRtlChar));

        var ordered = isRtl
            ? line.OrderByDescending(w => w.BoundingBox.Left)
            : line.OrderBy(w => w.BoundingBox.Left);

        var words = ordered.Select(w =>
        {
            var normalized = w.Text.Normalize(NormalizationForm.FormKC); // فك الـ ligatures أولاً
            return normalized.Any(IsStrongRtlChar)
                ? new string(normalized.Reverse().ToArray())              // بعدين اعكس الحروف الأساسية
                : normalized;
        });

        return string.Join(" ", words);
    }
    private static bool IsStrongRtlChar(char c) =>
        (c >= '\u0590' && c <= '\u08FF') ||   // Hebrew + Arabic + Arabic Supplement
        (c >= '\uFB1D' && c <= '\uFDFF') ||   // Hebrew/Arabic presentation forms A
        (c >= '\uFE70' && c <= '\uFEFF');     // Arabic presentation forms B
}