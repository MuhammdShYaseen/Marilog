using Marilog.Domain.Common;

namespace Marilog.Domain.Entities.SystemEntities
{
    public class Currency : Entity
    {
        public const string PivotCurrencyCode = "USD";
        public string CurrencyCode { get; private set; } = null!;  // ISO 4217 e.g. USD
        public string CurrencyName { get; private set; } = null!;  // e.g. US Dollar
        public string? Symbol { get; private set; }        // e.g. $
        public decimal ExchangeRate { get; private set; }  // USD per 1 unit (e.g. AED = 0.27229408)
        public bool IsBaseCurrency { get; private set; }

        private Currency () { }
        public static Currency Create(string currencyCode, string currencyName,
            decimal exchangeRate, string? symbol = null, bool isBaseCurrency = false)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(currencyCode);

            ArgumentException.ThrowIfNullOrWhiteSpace(currencyName);

            if (exchangeRate <= 0) throw new ArgumentException("ExchangeRate must be positive.");

            if (currencyCode.ToUpperInvariant() == PivotCurrencyCode && exchangeRate != 1m)
                throw new ArgumentException("USD rate must be 1.");

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

            if (CurrencyCode == PivotCurrencyCode && exchangeRate != 1m)
                throw new InvalidOperationException("USD rate must be 1.");

            CurrencyName = currencyName;
            ExchangeRate = exchangeRate;
            Symbol = symbol;
            Touch();
        }

        public void UpdateRate(decimal newRate)
        {
            if (newRate <= 0) throw new ArgumentException("ExchangeRate must be positive.");
            if (CurrencyCode == PivotCurrencyCode && newRate != 1m)
                throw new InvalidOperationException("USD rate must be 1.");
            ExchangeRate = newRate;
            Touch();
        }

        public void SetAsBase()
        {
            IsBaseCurrency = true;
            Touch();
        }

        public void UnSetBase() 
        {  
            IsBaseCurrency = false;
            Touch();
        }
    }
}
