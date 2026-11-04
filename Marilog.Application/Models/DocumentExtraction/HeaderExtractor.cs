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
            @"INVOICE\s*(?:NUMBER|NO|#)", @"INV\.?\s*NO", @"TI\s*#", @"BILL\s*NO", @"DOC\s*REF", @"TAX\s+INVOICE");

        private static readonly Regex[] DocDateLabels = Labels(@"INVOICE\s*(?:DATE|DT)", @"DATE", @"DT");
        private static readonly Regex[] DueDateLabels = Labels(@"DUE\s*DATE");

        private static readonly Regex[] PortLabels = Labels(
            @"DELIVERY\s*PORT", @"CALLING\s*PORT", @"PORT\s*OF\s*DISCHARGE", @"PORT\s*:");

        private static readonly Regex PaymentTerms = new(@"PAYMENT\s*TERMS\D{0,10}?(\d{1,3})\s*DAYS?", Opts);
        private static readonly Regex TotalWord = new(@"\bTOTAL\b", Opts);
        private static readonly Regex NumberInToken = new(@"\d[\d,]*(?:\.\d+)?", RegexOptions.Compiled);
        private static readonly Regex ImoRegex = new(@"IMO\D{0,15}?(\d{7})(?!\d)", Opts);

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

            var (total, totalCurrency) = FindTotal(lines);

            return new ExtractedHeader(
                Title: Titles.FirstOrDefault(t => text.Contains(t, StringComparison.OrdinalIgnoreCase)),
                DocNumber: FindValue(lines, DocNumberLabels, v => v.Any(char.IsDigit)),
                DocDate: docDate,
                DocDue: docDue,
                TotalAmount: total,
                Currency: totalCurrency ?? FirstCurrency(lines.SelectMany(l => l.Words)),
                Imo: FindImo(text),
                PortText: FindValue(lines, PortLabels, v => v.Any(char.IsLetter)));
        }

        // ── Label → value ────────────────────────────────────────────────
        // Value = rest of the label's segment, or the next segment when the rest is empty.
        private static string? FindValue(IReadOnlyList<TextLine> lines, Regex[] labels, Func<string, bool> accept)
        {
            foreach (var line in lines)
            {
                var segments = line.Segments();
                for (var i = 0; i < segments.Count; i++)
                {
                    foreach (var label in labels)
                    {
                        var m = label.Match(segments[i]);
                        if (!m.Success) continue;

                        var value = Clean(segments[i][m.Length..]);
                        if (value.Length == 0 && i + 1 < segments.Count)
                            value = Clean(segments[i + 1]);

                        if (value.Length > 0 && accept(value)) return value;
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
        private static (decimal? Amount, string? Currency) FindTotal(IReadOnlyList<TextLine> lines)
        {
            decimal? amount = null;
            string? currency = null;

            foreach (var line in lines)
            {
                var idx = line.Words.FindIndex(w => TotalWord.IsMatch(w.Text));
                if (idx < 0) continue;

                var found = FirstNumber(line.Words.Skip(idx + 1)) ?? FirstNumber(line.Words.Take(idx).Reverse());
                if (found is null) continue;

                amount = found;
                currency = FirstCurrency(line.Words);
            }

            return (amount, currency);
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
    }
}
