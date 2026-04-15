using Marilog.Domain.Common;

namespace Marilog.Domain.Entities.SystemEntities
{
    public class Currency : Entity
    {
        public string CurrencyCode { get; private set; } = null!;  // ISO 4217 e.g. USD
        public string CurrencyName { get; private set; } = null!;  // e.g. US Dollar
        public string? Symbol { get; private set; }        // e.g. $
        public decimal ExchangeRate { get; private set; }  // rate vs base currency
        public bool IsBaseCurrency { get; private set; }

        private Currency () { }
        public static Currency Create(string currencyCode, string currencyName,
            decimal exchangeRate, string? symbol = null, bool isBaseCurrency = false)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(currencyCode);
            ArgumentException.ThrowIfNullOrWhiteSpace(currencyName);
            if (exchangeRate <= 0) throw new ArgumentException("ExchangeRate must be positive.");

            return new Currency
            {
                CurrencyCode = currencyCode.ToUpperInvariant(),
                CurrencyName = currencyName,
                ExchangeRate = exchangeRate,
                Symbol = symbol,
                IsBaseCurrency = isBaseCurrency
            };
        }

        public void Update(string currencyName, decimal exchangeRate, string? symbol = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(currencyName);
            if (exchangeRate <= 0) throw new ArgumentException("ExchangeRate must be positive.");

            CurrencyName = currencyName;
            ExchangeRate = exchangeRate;
            Symbol = symbol;
            Touch();
        }

        public void UpdateRate(decimal newRate)
        {
            if (newRate <= 0) throw new ArgumentException("ExchangeRate must be positive.");
            if (IsBaseCurrency) throw new InvalidOperationException("Cannot change rate of base currency.");
            ExchangeRate = newRate;
            Touch();
        }

        public void SetAsBase()
        {
            IsBaseCurrency = true;
            ExchangeRate = 1m;
            Touch();
        }
    }
}
