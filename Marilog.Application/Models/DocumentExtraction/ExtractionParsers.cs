using System.Globalization;
using System.Text.RegularExpressions;

namespace Marilog.Application.Models.DocumentExtraction
{
    public static class ExtractionParsers
    {
        // ── Amounts ───────────────────────────────────────────────────────
        // "0,01,500.00"  "7000/="  "$ 72,800.00"  "AED 2,970.00"  "1,230 AED"  "(250.00)"
        private static readonly Regex AmountRegex = new(
            @"^(?<pre>[A-Z]{3}|US\s?\$|\$|€|£)?\s*(?<num>\(?-?\d[\d,]*(?:\.\d+)?\)?)\s*(?<post>[A-Z]{3})?\s*(?:/[=\-])?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static bool TryParseAmount(string? raw, out decimal value, out string? currency)
        {
            value = 0;
            currency = null;
            if (string.IsNullOrWhiteSpace(raw)) return false;

            var m = AmountRegex.Match(raw.Trim());
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

        // ── Dates (day first) ─────────────────────────────────────────────
        // "02/11/2026"  "07.09.2026"  "07-10-2026"  "DATE:07.09.2026"
        private static readonly Regex NumericDate = new(
            @"(?<!\d)(?<d>\d{1,2})[./-](?<m>\d{1,2})[./-](?<y>\d{4}|\d{2})(?!\d)",
            RegexOptions.Compiled);

        // "21ST OCT 2026"  "28-SEP-2026"  "03/NOV/2026"  "28 Sep 26"
        private static readonly Regex NamedMonthDate = new(
            @"(?<!\d)(?<d>\d{1,2})\s*(?:ST|ND|RD|TH)?[\s./-]*(?<m>JAN|FEB|MAR|APR|MAY|JUN|JUL|AUG|SEP|OCT|NOV|DEC)[A-Z]*\.?[\s./-]*(?<y>\d{4}|\d{2})(?!\d)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly string[] Months =
            ["JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC"];

        public static bool TryFindDate(string? text, out DateOnly date)
        {
            date = default;
            if (string.IsNullOrWhiteSpace(text)) return false;

            foreach (Match m in NamedMonthDate.Matches(text))
            {
                var month = Array.IndexOf(Months, m.Groups["m"].Value.ToUpperInvariant()) + 1;
                if (TryBuild(m.Groups["d"].Value, month, m.Groups["y"].Value, out date)) return true;
            }

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
                : new string(text.Where(char.IsLetterOrDigit).Select(char.ToUpperInvariant).ToArray());
    }
}