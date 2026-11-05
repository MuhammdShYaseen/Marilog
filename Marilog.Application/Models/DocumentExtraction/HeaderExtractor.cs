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
            @"(?:TAX\s+)?INVOICE\s*(?:NUMBER|NO|NR|N[°º]|#)\.?(?:\s*(?:&|AND)\s*DATE)?", @"(?:E?PDA|FDA|DA)\s*(?:NO|#)",
            @"INV\.?\s*NO", @"TI\s*#", @"BILL\s*NO", @"DOC\s*REF", @"TAX\s+INVOICE");
        private static readonly Regex[] DocDateLabels = Labels(
            @"INVOICE\s*(?:NUMBER|NO|NR)\.?\s*(?:&|AND)\s*DATE",
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
        private static readonly Regex CurrencyLabel = new(@"\b(?:CURRENCY|CURR|CCY)\b", Opts);

        private static readonly Regex PaymentTerms = new(@"PAYMENT\s*TERMS\D{0,10}?(\d{1,3})\s*DAYS?|(\d{1,3})\s*DAYS?\s*CREDIT", Opts);
        private static readonly Regex TotalWord = new(@"\bTOTAL\b", Opts);
        private static readonly Regex NumberInToken = new(@"\d[\d,]*(?:\.\d+)?", RegexOptions.Compiled);
        private static readonly Regex ImoRegex = new(@"IMO\D{0,15}?(\d{7})(?!\d)", Opts);
        private static readonly Regex BalanceWord = new(@"\bBALANCE\b", Opts);

        // A number printed right after the title word, with no "No" label: "INVOICE RSM 3077".
        private static readonly Regex TitleNumber = new(
            @"^(?:TAX\s+|COMMERCIAL\s+|PROFORMA\s+)?INVOICE\s+(?!(?:TO|DATE|NO|NUMBER|AMOUNT|TOTAL|VALUE)\b)(?<v>\S+(?:\s\S+)?)$", Opts);

        // Most specific first.
        private static readonly string[] Titles =
        [
            "FINAL DISBURSEMENT ACCOUNT", "PROFORMA DISBURSEMENT ACCOUNT", "ESTIMATED PDA",
            "STATEMENT OF ACCOUNT", "SOA",
            "PAYMENT ADVICE", "REMITTANCE ADVICE", "DEBIT ADVICE", "PAYMENT CONFIRMATION", "TRANSFER CONFIRMATION",
            "SWIFT COPY", "MT103",
            "COMMERCIAL INVOICE", "PROFORMA INVOICE", "TAX INVOICE",
            "DEBIT NOTE", "CREDIT NOTE", "INVOICE"
        ];

        // Other wordings of the same title.
        private static readonly Dictionary<string, string> TitleAliases = new(StringComparer.OrdinalIgnoreCase)
        {
            ["SOA"] = "STATEMENT OF ACCOUNT",
            ["REMITTANCE ADVICE"] = "PAYMENT ADVICE",
            ["DEBIT ADVICE"] = "PAYMENT ADVICE",
            ["PAYMENT CONFIRMATION"] = "PAYMENT ADVICE",
            ["TRANSFER CONFIRMATION"] = "PAYMENT ADVICE",
            ["SWIFT COPY"] = "PAYMENT ADVICE",
            ["MT103"] = "PAYMENT ADVICE"
        };

        public static ExtractedHeader Extract(IReadOnlyList<TextLine> lines, CurrencyCatalog currencies)
        {
            var text = string.Join('\n', lines.Select(l => l.Text));

            var docDate = ParseDate(FindValue(lines, DocDateLabels, IsDate)) ?? DateBesideNumber(lines);
            var docDue = ParseDate(FindValue(lines, DueDateLabels, IsDate));

            // No due date printed: "PAYMENT TERMS 30 DAYS" or "10 DAYS CREDIT" added to the document date.
            if (docDue is null && docDate is not null)
            {
                var terms = PaymentTerms.Match(text);
                if (terms.Success)
                    docDue = docDate.Value.AddDays(int.Parse(terms.Groups[1].Success ? terms.Groups[1].Value : terms.Groups[2].Value));
            }

            var (total, totalCurrency) = FindTotal(lines, TotalWord, currencies);
            if (total is null)
                (total, totalCurrency) = FindTotal(lines, BalanceWord, currencies);
            if (total is null)
                (total, totalCurrency) = LastAmountLine(lines, currencies);

            return new ExtractedHeader(
                Title: FindTitle(lines),
                DocNumber: StripLeadingLabel(
                    FindValue(lines, DocNumberLabels, v => v.Any(char.IsDigit))
                    ?? FindValue(lines, DocNumberFallbackLabels, v => v.Any(char.IsDigit)),
                    DocNumberLabels) ?? FindTitleNumber(lines),
                DocDate: docDate,
                DocDue: docDue,
                TotalAmount: total,
                Currency: totalCurrency ?? FirstCurrency(lines, currencies),
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

                        // 2. Label row above value rows: "Date | Ref | Page" over "25-Jan-2025 | 2412-158 DP-C | 1/1",
                        //    or "Invoice No. & Date" over the number, with the date one line lower.
                        if (rest.Length == 0)
                        {
                            for (var n = li + 1; n <= li + 2 && n < lines.Count; n++)
                            {
                                var below = lines[n].SegmentSpans()
                                    .FirstOrDefault(s => s.Left <= segments[i].Right && s.Right >= segments[i].Left);
                                if (below is null) break;

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

        // No date label: the date printed on the number's line, "Invoice Nr.: | 26080992 | 11-08-2026".
        private static DateOnly? DateBesideNumber(IReadOnlyList<TextLine> lines)
        {
            foreach (var line in lines)
            {
                var segments = line.SegmentSpans();

                for (var i = 0; i < segments.Count; i++)
                {
                    if (!DocNumberLabels.Any(l => l.IsMatch(segments[i].Text))) continue;

                    foreach (var s in segments.Skip(i))
                        if (ParseDate(s.Text) is { } date) return date;
                }
            }
            return null;
        }

        // ── Total: last line holding the word TOTAL and a number ─────────
        // Number after TOTAL ("TOTAL AED 12,572.08"), else before it ("USD 0,07,675.00 :TOTAL").
        private static (decimal? Amount, string? Currency) FindTotal(IReadOnlyList<TextLine> lines, Regex word, CurrencyCatalog currencies)
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
                        .Where(n => n >= 0 && n < lines.Count && IsNumbersOnly(lines[n], currencies))
                        .Select(n => lines[n])
                        .FirstOrDefault();

                    if (figures is not null)
                        found = FirstNonZero(Enumerable.Reverse(figures.Words));
                }

                if (found is null) continue;

                amount = found;
                currency = FirstCurrency([line], currencies);
            }

            return (amount, currency);
        }

        // No TOTAL or BALANCE word: the last line holding only a currency and an amount ("USD | 20,000.00").
        private static (decimal? Amount, string? Currency) LastAmountLine(IReadOnlyList<TextLine> lines, CurrencyCatalog currencies)
        {
            for (var li = lines.Count - 1; li >= 0; li--)
            {
                var line = lines[li];
                if (!IsNumbersOnly(line, currencies)) continue;

                var currency = FirstCurrency([line], currencies);
                var amount = FirstNonZero(Enumerable.Reverse(line.Words));
                if (currency is not null && amount is not null) return (amount, currency);
            }

            return (null, null);
        }

        private static bool IsNumbersOnly(TextLine line, CurrencyCatalog currencies) =>
            line.Words.All(w => ExtractionParsers.TryParseAmount(w.Text, out _, out _) || currencies.Of(w.Text) is not null);

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

        // First currency in the lines. One next to an amount or after a currency label wins over a bare one,
        // so "TOTAL ALL CHARGES USD 1,000" gives USD even if ALL (Albanian lek) is in the Currency table.
        private static string? FirstCurrency(IEnumerable<TextLine> lines, CurrencyCatalog currencies)
        {
            string? bare = null;

            foreach (var line in lines)
            {
                var w = line.Words;
                for (var i = 0; i < w.Count; i++)
                {
                    var code = currencies.Of(w[i].Text);
                    if (code is null) continue;

                    var anchored = HasDigit(w[i].Text)
                        || (i > 0 && HasDigit(w[i - 1].Text))
                        || (i + 1 < w.Count && HasDigit(w[i + 1].Text))
                        || CurrencyLabel.IsMatch(string.Join(' ', w.Take(i).TakeLast(2).Select(x => x.Text)));

                    if (anchored) return code;
                    bare ??= code;
                }
            }

            return bare;
        }

        private static bool HasDigit(string s) => s.Any(char.IsDigit);

        // ── Title ────────────────────────────────────────────────────────
        // The earliest line holding a title phrase; the most specific phrase on that line.
        // A footer like "see our statement of account" no longer overrides the real title above it.
        private static string? FindTitle(IReadOnlyList<TextLine> lines)
        {
            foreach (var line in lines)
            {
                var title = Titles.FirstOrDefault(t => HasPhrase(line.Text, t));
                if (title is not null) return TitleAliases.GetValueOrDefault(title, title);
            }

            return null;
        }

        // Whole words only: "SOA" must not match inside "SOAP".
        private static bool HasPhrase(string text, string phrase) =>
            Regex.IsMatch(text, $@"(?<![A-Z0-9]){Regex.Escape(phrase).Replace(@"\ ", @"\s+")}(?![A-Z0-9])", RegexOptions.IgnoreCase);

        private static string? FindTitleNumber(IReadOnlyList<TextLine> lines) =>
            lines.SelectMany(l => l.SegmentSpans())
                .Select(s => TitleNumber.Match(s.Text))
                .Where(m => m.Success && m.Groups["v"].Value.Any(char.IsDigit))
                .Select(m => m.Groups["v"].Value.Trim())
                .FirstOrDefault();

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