using Marilog.OCR.Worker.Abstractions;
using System.Text;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace Marilog.OCR.Worker.Infrastructure;

public sealed class PdfDirectTextExtractor : IPdfDirectTextExtractor
{
    // Fast compiled regex matching any Arabic character range
    private static readonly Regex ArabicRegex = new Regex(
        @"[\u0590-\u08FF\uFB1D-\uFDFF\uFE70-\uFEFF]",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public string ExtractText(string filePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException("Target PDF file not found.", filePath);

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
                // Scanned page or missing font-maps; skip safely in production worker
                continue;
            }

            if (words.Count == 0)
                continue;

            var lines = GroupIntoLines(words);

            foreach (var line in lines)
            {
                cancellationToken.ThrowIfCancellationRequested();
                sb.AppendLine(BuildLineText(line));
            }

            sb.AppendLine();
        }

        return sb.ToString().TrimEnd();
    }

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
                lines.Add(new List<Word> { word });
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

    private static string BuildLineText(List<Word> line)
    {
        bool isRtlLine = line.Any(w => ArabicRegex.IsMatch(w.Text));

        var orderedWords = isRtlLine
            ? line.OrderByDescending(w => w.BoundingBox.Left)
            : line.OrderBy(w => w.BoundingBox.Left);

        var processedWords = new List<string>(orderedWords.Count());

        foreach (var word in orderedWords)
        {
            var normalized = word.Text.Normalize(NormalizationForm.FormKC);

            if (ArabicRegex.IsMatch(normalized))
            {
                char[] charArray = normalized.ToCharArray();
                Array.Reverse(charArray);

                // تصحيح مشكلة اللام ألف بعد العكس
                // بما أن النص كان (ل -> أ) وانعكس ليصبح (أ -> ل)، سنقوم بإعادته إلى (ل -> أ)
                for (int i = 0; i < charArray.Length - 1; i++)
                {
                    // الشرط الأول (القديم كما هو بدون تعديل)
                    if ((charArray[i] == 'أ' || charArray[i] == 'إ' || charArray[i] == 'آ')
                        && charArray[i + 1] == 'ل')
                    {
                        // نقوم بتبديل الحرفين لإعادة الترتيب الصحيح لـ (لا)
                        char temp = charArray[i];
                        charArray[i] = charArray[i + 1];
                        charArray[i + 1] = temp;
                        i++; // قفز خطوة لتجنب إعادة تبديل نفس الحرف
                    }
                    // الشرط الثاني: إذا كانت (أ أو إ أو آ أو ا) متبوعة بـ (ل) في وسط الكلمة (يوجد حرف قبلها وحرف بعدها)
                    else if (i > 0 && i < charArray.Length - 2
                        && (charArray[i] == 'ا' || charArray[i] == 'أ' || charArray[i] == 'إ' || charArray[i] == 'آ')
                        && charArray[i + 1] == 'ل')
                    {
                        // نقوم بتبديلهم ليصبحوا ل + ا
                        char temp = charArray[i];
                        charArray[i] = charArray[i + 1];
                        charArray[i + 1] = temp;
                        i++;
                    }
                }

                processedWords.Add(new string(charArray));
            }
            else
            {
                processedWords.Add(normalized);
            }
        }

        return string.Join(" ", processedWords);
    }
}