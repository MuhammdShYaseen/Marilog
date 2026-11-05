using Marilog.Application.Models.DocumentExtraction;
using Marilog.Contracts.DTOs.Responses;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Marilog.Application.DocumentExtraction
{
    public static class HeaderExtractor
    {
        private const RegexOptions Opts = RegexOptions.Compiled | RegexOptions.IgnoreCase;

        // A label must start its segment and not run into another word: "DATE" ≠ "Due Date", "DATED".
        private static Regex[] Labels(params string[] patterns) =>
            patterns.Select(p => new Regex($@"^(?:{p})(?![A-Z0-9])", Opts)).ToArray();

        private static readonly Regex[] DocNumberLabels = Labels(
    @"(?:TAX\s+)?INVOICE\s*(?:NUMBER|NO|#)", @"(?:E?PDA|FDA|DA)\s*(?:NO|#)",
    @"INV\.?\s*NO", @"TI\s*#", @"BILL\s*NO", @"DOC\s*REF", @"TAX\s+INVOICE");
        private static readonly Regex[] DocDateLabels = Labels(
     @"INVOICE\s*(?:DATE|DT)",
     @"(?:E?PDA|FDA|DA|ESTIMATE)\s*DATE(?:\s*&\s*VERSION)?",
     @"DATED",
     @"DATE",
     @"DT");
        private static readonly Regex[] DueDateLabels = Labels(@"DUE\s*DATE");
        private static readonly Regex[] DocNumberFallbackLabels = Labels(
     @"OUR\s*REF(?:ERENCE)?", @"REF(?:ERENCE)?\s*(?:NO|#)", @"REF(?:ERENCE)?");
        private static readonly Regex[] PortLabels = Labels(
            @"DELIVERY\s*PORT", @"CALLING\s*PORT", @"PORT\s*OF\s*DISCHARGE", @"PORT\s*:");

        private static readonly Regex PaymentTerms = new(@"PAYMENT\s*TERMS\D{0,10}?(\d{1,3})\s*DAYS?", Opts);
        private static readonly Regex TotalWord = new(@"\bTOTAL\b", Opts);
        private static readonly Regex NumberInToken = new(@"\d[\d,]*(?:\.\d+)?", RegexOptions.Compiled);
        private static readonly Regex ImoRegex = new(@"IMO\D{0,15}?(\d{7})(?!\d)", Opts);
        private static readonly Regex BalanceWord = new(@"\bBALANCE\b", Opts);

        // Most specific first.
        private static readonly string[] Titles =
        [
            "FINAL DISBURSEMENT ACCOUNT", "PROFORMA DISBURSEMENT ACCOUNT", "ESTIMATED PDA",
            "STATEMENT OF ACCOUNT", "COMMERCIAL INVOICE", "PROFORMA INVOICE", "TAX INVOICE",
            "DEBIT NOTE", "CREDIT NOTE", "INVOICE"
        ];

        // Temporary — step 5 replaces this with the Currency table.
        private static readonly HashSet<string> Currencies = new(StringComparer.OrdinalIgnoreCase)
        {
            "AED", "USD", "EUR", "GBP", "INR", "BHD", "SAR", "OMR", "QAR", "KWD", "EGP", "JOD"
        };

        public static ExtractedHeader Extract(IReadOnlyList<TextLine> lines)
        {
            var text = string.Join('\n', lines.Select(l => l.Text));

            var docDate = ParseDate(FindValue(lines, DocDateLabels, IsDate));
            var docDue = ParseDate(FindValue(lines, DueDateLabels, IsDate));

            if (docDue is null && docDate is not null)
            {
                var terms = PaymentTerms.Match(text);
                if (terms.Success)
                    docDue = docDate.Value.AddDays(int.Parse(terms.Groups[1].Value));
            }

            var (total, totalCurrency) = FindTotal(lines, TotalWord);
            if (total is null)
                (total, totalCurrency) = FindTotal(lines, BalanceWord);

            return new ExtractedHeader(
                Title: Titles.FirstOrDefault(t => text.Contains(t, StringComparison.OrdinalIgnoreCase)),
                DocNumber: StripLeadingLabel(
                    FindValue(lines, DocNumberLabels, v => v.Any(char.IsDigit))
                    ?? FindValue(lines, DocNumberFallbackLabels, v => v.Any(char.IsDigit)),
                    DocNumberLabels),
                DocDate: docDate,
                DocDue: docDue,
                TotalAmount: total,
                Currency: totalCurrency ?? FirstCurrency(lines.SelectMany(l => l.Words)),
                Imo: FindImo(text),
                PortText: FindValue(lines, PortLabels, v => v.Any(char.IsLetter) && !v.Contains(':')));
        }

        // ── Label → value ────────────────────────────────────────────────
        // Value = rest of the label's segment, or the next segment when the rest is empty.
        private static string? FindValue(IReadOnlyList<TextLine> lines, Regex[] labels, Func<string, bool> accept)
        {
            for (var li = 0; li < lines.Count; li++)
            {
                var segments = lines[li].SegmentSpans();

                for (var i = 0; i < segments.Count; i++)
                {
                    foreach (var label in labels)
                    {
                        var m = label.Match(segments[i].Text);
                        if (!m.Success) continue;

                        // 1. Rest of the label's segment, else the next segment on the same line.
                        var rest = Clean(segments[i].Text[m.Length..]);
                        var value = rest.Length == 0 && i + 1 < segments.Count ? Clean(segments[i + 1].Text) : rest;
                        if (value.Length > 0 && accept(value)) return value;

                        // 2. Label row above value row: "Date | Ref | Page" over "25-Jan-2025 | 2412-158 DP-C | 1/1".
                        if (rest.Length == 0 && li + 1 < lines.Count)
                        {
                            var below = lines[li + 1].SegmentSpans()
                                .FirstOrDefault(s => s.Left <= segments[i].Right && s.Right >= segments[i].Left);

                            if (below is not null)
                            {
                                var v = Clean(below.Text);
                                if (v.Length > 0 && accept(v)) return v;
                            }
                        }

                        // 3. Value printed slightly higher than its label: "PDA No:" with "ASG20262009" on the line above.
                        if (rest.Length == 0 && li > 0)
                        {
                            var lineHeight = lines[li].Top - lines[li].Bottom;
                            var above = lines[li - 1];

                            if (above.CenterY - lines[li].CenterY < lineHeight * 2)
                            {
                                var candidate = above.SegmentSpans().FirstOrDefault(s => s.Left >= segments[i].Left);
                                if (candidate is not null)
                                {
                                    var v = Clean(candidate.Text);
                                    if (v.Length > 0 && accept(v)) return v;
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        private static string Clean(string s) => s.Trim().TrimStart(':', '#', '-', '.', ' ').Trim();

        private static bool IsDate(string v) => ExtractionParsers.TryFindDate(v, out _);

        private static DateOnly? ParseDate(string? v) =>
            ExtractionParsers.TryFindDate(v, out var d) ? d : null;

        // ── Total: last line holding the word TOTAL and a number ─────────
        // Number after TOTAL ("TOTAL AED 12,572.08"), else before it ("USD 0,07,675.00 :TOTAL").
        private static (decimal? Amount, string? Currency) FindTotal(IReadOnlyList<TextLine> lines, Regex word)
        {
            decimal? amount = null;
            string? currency = null;

            for (var li = 0; li < lines.Count; li++)
            {
                var line = lines[li];
                var idx = line.Words.FindIndex(w => word.IsMatch(w.Text));
                if (idx < 0) continue;

                // After the word: last non-zero number. Before it: the nearest non-zero number.
                var found = FirstNonZero(line.Words.Skip(idx + 1).Reverse())
                         ?? FirstNonZero(line.Words.Take(idx).Reverse());

                // "TOTAL:-" alone, its figures on a numbers-only line just above or below.
                if (found is null)
                {
                    var figures = new[] { li - 1, li + 1 }
                        .Where(n => n >= 0 && n < lines.Count && IsNumbersOnly(lines[n]))
                        .Select(n => lines[n])
                        .FirstOrDefault();

                    if (figures is not null)
                        found = FirstNonZero(Enumerable.Reverse(figures.Words));
                }

                if (found is null) continue;

                amount = found;
                currency = FirstCurrency(line.Words);
            }

            return (amount, currency);
        }

        private static bool IsNumbersOnly(TextLine line) =>
            line.Words.All(w => ExtractionParsers.TryParseAmount(w.Text, out _, out _) || CurrencyOf(w.Text) is not null);

        private static decimal? FirstNonZero(IEnumerable<PdfWord> words)
        {
            foreach (var w in words)
            {
                var m = NumberInToken.Match(w.Text);
                if (m.Success &&
                    decimal.TryParse(m.Value.Replace(",", ""), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var v) &&
                    v != 0)
                    return v;
            }
            return null;
        }
        private static decimal? FirstNumber(IEnumerable<PdfWord> words)
        {
            foreach (var w in words)
            {
                var m = NumberInToken.Match(w.Text);
                if (m.Success && decimal.TryParse(m.Value.Replace(",", ""),
                        NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var v))
                    return v;
            }
            return null;
        }

        private static string? FirstCurrency(IEnumerable<PdfWord> words) =>
            words.Select(w => CurrencyOf(w.Text)).FirstOrDefault(c => c is not null);

        private static string? CurrencyOf(string word)
        {
            if (word.Contains('$')) return "USD";
            if (word.Contains('€')) return "EUR";
            if (word.Contains('£')) return "GBP";

            var letters = new string(word.Where(char.IsLetter).ToArray());
            return Currencies.Contains(letters) ? letters.ToUpperInvariant() : null;
        }

        // ── IMO with check digit ─────────────────────────────────────────
        private static string? FindImo(string text)
        {
            foreach (Match m in ImoRegex.Matches(text))
                if (IsValidImo(m.Groups[1].Value)) return m.Groups[1].Value;
            return null;
        }

        private static bool IsValidImo(string imo)
        {
            var sum = 0;
            for (var i = 0; i < 6; i++) sum += (imo[i] - '0') * (7 - i);
            return sum % 10 == imo[6] - '0';
        }

        private static string? StripLeadingLabel(string? value, Regex[] labels)
        {
            if (value is null) return null;

            foreach (var label in labels)
            {
                var m = label.Match(value);
                if (!m.Success) continue;

                var rest = Clean(value[m.Length..]);
                if (rest.Any(char.IsDigit)) return rest;
            }

            return value;
        }
    }
}
