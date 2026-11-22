using Humanizer;
using System.Globalization;

namespace Marilog.Infrastructure.Services.Qpdf
{
    // 12,500.50 USD → "Twelve Thousand Five Hundred US Dollars and Fifty Cents".
    // Rounded to 2 decimals, the same as every amount printed on the template, so the words always match the figures.
    // Humanizer's ToWords works on whole numbers only, so the cents are written separately.
    public static class AmountInWords
    {
        private static readonly CultureInfo English = CultureInfo.InvariantCulture;   // Humanizer's neutral language is English

        // Name of 1/100 of the currency (singular, plural). A currency that isn't listed, or doesn't split into 100,
        // gets its cents as "50/100", which is right for every currency.
        private static readonly Dictionary<string, (string One, string Many)> Cents = new(StringComparer.OrdinalIgnoreCase)
        {
            ["USD"] = ("Cent", "Cents"),
            ["EUR"] = ("Cent", "Cents"),
            ["AUD"] = ("Cent", "Cents"),
            ["CAD"] = ("Cent", "Cents"),
            ["SGD"] = ("Cent", "Cents"),
            ["HKD"] = ("Cent", "Cents"),
            ["AED"] = ("Fils", "Fils"),
            ["SAR"] = ("Halala", "Halalas"),
            ["QAR"] = ("Dirham", "Dirhams"),
            ["GBP"] = ("Penny", "Pence"),
            ["EGP"] = ("Piastre", "Piastres"),
            ["INR"] = ("Paisa", "Paise"),
            ["TRY"] = ("Kurus", "Kurus"),
            ["CNY"] = ("Fen", "Fen")
        };

        public static string Write(decimal amount, string currencyCode, string? currencyName)
        {
            var value = Math.Round(Math.Abs(amount), 2, MidpointRounding.AwayFromZero);
            var whole = (long)decimal.Truncate(value);
            var cents = (int)((value - whole) * 100);

            var name = string.IsNullOrWhiteSpace(currencyName) ? currencyCode : currencyName.Trim();
            var text = $"{Words(whole)} {(whole == 1 ? name : name.Pluralize())}";

            if (cents > 0)
            {
                text += Cents.TryGetValue(currencyCode, out var unit)
                    ? $" and {Words(cents)} {(cents == 1 ? unit.One : unit.Many)}"
                    : $" and {cents:00}/100";
            }

            return amount < 0 ? "Minus " + text : text;
        }

        // 1250 → "One Thousand Two Hundred Fifty" (no inner "and": the only "and" is the one before the cents).
        private static string Words(long number)
            => English.TextInfo.ToTitleCase(number.ToWords(English, addAnd: false));
    }
}