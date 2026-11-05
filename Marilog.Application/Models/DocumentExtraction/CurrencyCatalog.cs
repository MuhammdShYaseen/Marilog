using Marilog.Domain.Entities.SystemEntities;
using System.Text.RegularExpressions;

namespace Marilog.Application.Models.DocumentExtraction
{
    // Currency codes and symbols from the Currency table — nothing hard-coded.
    public sealed class CurrencyCatalog
    {
        private readonly HashSet<string> _codes;
        private readonly Dictionary<string, string> _symbols;

        public CurrencyCatalog(IEnumerable<(string Code, string? Symbol)> currencies)
        {
            var list = currencies
                .Where(c => !string.IsNullOrWhiteSpace(c.Code))
                .Select(c => (Code: c.Code.Trim().ToUpperInvariant(), Symbol: Token(c.Symbol)))
                .ToList();

            _codes = list.Select(c => c.Code).ToHashSet();

            // A symbol shared by several currencies ("$") goes to the pivot currency when it is one of them; otherwise it is ignored.
            _symbols = list
                // A one-letter symbol ("S") would match inside part numbers like "S46", so it is ignored.
                .Where(c => c.Symbol.Length > 0 && !_codes.Contains(c.Symbol) && !(c.Symbol.Length == 1 && char.IsLetter(c.Symbol[0])))
                .GroupBy(c => c.Symbol)
                .Select(g => (Symbol: g.Key, Code: g.Count() == 1
                    ? g.First().Code
                    : g.Select(x => x.Code).FirstOrDefault(x => x == Currency.PivotCurrencyCode)))
                .Where(x => x.Code is not null)
                .ToDictionary(x => x.Symbol, x => x.Code!);

            // "US$" is printed for USD on many invoices.
            if (_codes.Contains("USD"))
                _symbols.TryAdd("US$", "USD");
        }

        // Letters on both sides of a digit = a part or reference number ("0101Q0013-002", "H=163MM"), never a currency.
        private static readonly Regex PartNumber = new(@"\p{L}.*\d.*\p{L}|\d.*\p{L}.*\d", RegexOptions.Compiled);

        // The currency a word stands for: "USD", "AED:", "$72,800.00", "US$", "(EUR)", "USD#". Null when none.
        public string? Of(string? word)
        {
            if (word is null || PartNumber.IsMatch(word)) return null;

            var token = Token(word);
            if (token.Length == 0) return null;
            if (_codes.Contains(token)) return token;
            return _symbols.TryGetValue(token, out var code) ? code : null;
        }

        // Drops digits, spaces and number punctuation: "US$ 72,800.00" → "US$".
        private static string Token(string? text) =>
            string.IsNullOrWhiteSpace(text)
                ? ""
                : new string(ExtractionParsers.NormalizeDigits(text)
                    .Where(ch => !char.IsDigit(ch) && !char.IsWhiteSpace(ch) && !",.:;()/=-#".Contains(ch))
                    .Select(char.ToUpperInvariant)
                    .ToArray());
    }
}