using System.Globalization;
using System.Text.RegularExpressions;

namespace Marilog.Application.Models.DocumentExtraction
{
    public static class ExtractionParsers
    {
        // ── Digits ────────────────────────────────────────────────────────
        // Arabic-Indic (٠-٩) and Persian (۰-۹) digits, Arabic decimal (٫) and thousands (٬) separators → ASCII.
        // Without this, "\d" would match them and int.Parse would throw.
        public static string NormalizeDigits(string? text)
        {
            if (string.IsNullOrEmpty(text)) return text ?? "";

            var chars = text.ToCharArray();
            for (var i = 0; i < chars.Length; i++)
            {
                var c = chars[i];
                if (c >= '\u0660' && c <= '\u0669') chars[i] = (char)('0' + (c - '\u0660'));
                else if (c >= '\u06F0' && c <= '\u06F9') chars[i] = (char)('0' + (c - '\u06F0'));
                else if (c == '\u066B') chars[i] = '.';
                else if (c == '\u066C') chars[i] = ',';
            }
            return new string(chars);
        }

        // ── Text cleanup ──────────────────────────────────────────────────
        // "I n v o i c e" → "Invoice". Letters only, 3+ in a row, so numbers and short pairs stay as they are.
        private static readonly Regex SpacedLetters = new(@"(?<!\S)(?:[A-Za-z] ){2,}[A-Za-z](?!\S)", RegexOptions.Compiled);

        public static string CollapseSpacedLetters(string text) =>
            SpacedLetters.Replace(text, m => m.Value.Replace(" ", ""));

        // "ALSHARIF GROUP W.L.L." → "ALSHARIF GROUP", "BULK ASIA PTE LTD" → "BULK ASIA"
        private static readonly Regex LegalSuffix = new(
            @"(?:[\s,.\-()]+(?:W\.?\s?L\.?\s?L|L\.?\s?L\.?\s?C|F\.?\s?Z\.?\s?E|F\.?\s?Z\.?\s?C\.?\s?O|FZ-?LLC|DMCC|SPS|PTE|PVT|LTD|LIMITED|S\.?\s?A|INC|CO|B\.?\s?V|N\.?\s?V)\.?\)?)+\s*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static string StripLegalSuffix(string? name) =>
            string.IsNullOrWhiteSpace(name) ? "" : LegalSuffix.Replace(name.Trim(), "");

        // ── Amounts ───────────────────────────────────────────────────────
        // "0,01,500.00"  "7000/="  "$ 72,800.00"  "AED 2,970.00"  "1,230 AED"  "(250.00)"
        private static readonly Regex AmountRegex = new(
            @"^(?<pre>[A-Z]{3}|US\s?\$|\$|€|£)?\s*(?<num>\(?-?[0-9][0-9,]*(?:\.[0-9]+)?\)?)\s*(?<post>[A-Z]{3})?\s*(?:/[=\-])?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static bool TryParseAmount(string? raw, out decimal value, out string? currency)
        {
            value = 0;
            currency = null;
            if (string.IsNullOrWhiteSpace(raw)) return false;

            var m = AmountRegex.Match(NormalizeDigits(raw).Trim());
            if (!m.Success) return false;

            var num = m.Groups["num"].Value;
            var negative = num.Contains('(') || num.Contains('-');
            var digits = num.Replace("(", "").Replace(")", "").Replace("-", "").Replace(",", "");

            if (!decimal.TryParse(digits, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out value))
                return false;

            if (negative) value = -value;

            var token = m.Groups["pre"].Success ? m.Groups["pre"].Value : m.Groups["post"].Value;
            currency = MapCurrency(token);
            return true;
        }

        private static string? MapCurrency(string token) =>
            token.Replace(" ", "").ToUpperInvariant() switch
            {
                "" => null,
                "$" or "US$" => "USD",
                "€" => "EUR",
                "£" => "GBP",
                var code => code
            };

        // ── Dates ─────────────────────────────────────────────────────────
        // ISO, year first: "2026-09-20"
        private static readonly Regex IsoDate = new(
            @"(?<![0-9])(?<y>[0-9]{4})[./-](?<m>[0-9]{1,2})[./-](?<d>[0-9]{1,2})(?![0-9])",
            RegexOptions.Compiled);

        // Day first, named month: "21ST OCT 2026"  "28-SEP-2026"  "03/NOV/2026"  "28 Sep 26"  "08/SEPT./2026"
        private static readonly Regex NamedMonthDate = new(
            @"(?<![0-9])(?<d>[0-9]{1,2})\s*(?:ST|ND|RD|TH)?[\s./-]*(?<m>JAN|FEB|MAR|APR|MAY|JUN|JUL|AUG|SEP|OCT|NOV|DEC)[A-Z]*\.?[\s./-]*(?<y>[0-9]{4}|[0-9]{2})(?![0-9])",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Month first: "Sep 20, 2026"  "September 20th 2026"
        private static readonly Regex MonthFirstDate = new(
            @"(?<![A-Z])(?<m>JAN|FEB|MAR|APR|MAY|JUN|JUL|AUG|SEP|OCT|NOV|DEC)[A-Z]*\.?\s+(?<d>[0-9]{1,2})(?:ST|ND|RD|TH)?,?\s+(?<y>[0-9]{4})(?![0-9])",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Numeric, day first: "02/11/2026"  "07.09.2026"  "07-10-2026"  "DATE:07.09.2026"
        private static readonly Regex NumericDate = new(
            @"(?<![0-9])(?<d>[0-9]{1,2})[./-](?<m>[0-9]{1,2})[./-](?<y>[0-9]{4}|[0-9]{2})(?![0-9])",
            RegexOptions.Compiled);

        private static readonly string[] Months =
            ["JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC"];

        public static bool TryFindDate(string? text, out DateOnly date)
        {
            date = default;
            if (string.IsNullOrWhiteSpace(text)) return false;

            text = NormalizeDigits(text);

            foreach (Match m in IsoDate.Matches(text))
                if (TryBuild(m.Groups["d"].Value, int.Parse(m.Groups["m"].Value), m.Groups["y"].Value, out date))
                    return true;

            foreach (Match m in NamedMonthDate.Matches(text))
                if (TryBuild(m.Groups["d"].Value, MonthOf(m.Groups["m"].Value), m.Groups["y"].Value, out date))
                    return true;

            foreach (Match m in MonthFirstDate.Matches(text))
                if (TryBuild(m.Groups["d"].Value, MonthOf(m.Groups["m"].Value), m.Groups["y"].Value, out date))
                    return true;

            foreach (Match m in NumericDate.Matches(text))
            {
                var d = m.Groups["d"].Value;
                var mo = m.Groups["m"].Value;
                var y = m.Groups["y"].Value;

                // Day first; swap only when that is impossible (e.g. 11/25/2026).
                if (TryBuild(d, int.Parse(mo), y, out date) || TryBuild(mo, int.Parse(d), y, out date))
                    return true;
            }

            return false;
        }

        private static int MonthOf(string name) =>
            Array.IndexOf(Months, name[..3].ToUpperInvariant()) + 1;

        private static bool TryBuild(string day, int month, string year, out DateOnly date)
        {
            date = default;
            var d = int.Parse(day);
            var y = int.Parse(year);
            if (y < 100) y += 2000;

            if (month is < 1 or > 12 || y is < 2000 or > 2100 || d < 1 || d > DateTime.DaysInMonth(y, month))
                return false;

            date = new DateOnly(y, month, d);
            return true;
        }

        // ── Identifiers (IBAN, TRN, GSTIN, account numbers) ──────────────
        // "AE 410351821323352248015" → "AE410351821323352248015"
        // "2 4 A B P P D 3 6 4 0"    → "24ABPPD3640"
        public static string NormalizeId(string? text) =>
            string.IsNullOrEmpty(text)
                ? ""
                : new string(NormalizeDigits(text).Where(char.IsLetterOrDigit).Select(char.ToUpperInvariant).ToArray());
    }
}