
namespace Marilog.Domain.ValueObjects.ReusableValueObjects
{
    public sealed class BankAccount : ValueObject
    {
        public string IBAN { get; private set; } = null!;
        public string BankName { get; private set; } = null!;
        public string? SwiftCode { get; private set; }
        public int CurrencyId { get; private set; }  // USD, EUR, AED...
        public string? AccountHolderName { get; private set; }
        public bool IsPrimary { get; private set; }

        private BankAccount() { } // EF Core

        public static BankAccount Create (string iban, string bankName, string? swiftCode,
            int currencyId, string? accountHolderName, bool isPrimary)
        {
            if (string.IsNullOrWhiteSpace(iban))
                throw new ArgumentNullException (nameof (iban));

            if (string.IsNullOrWhiteSpace(bankName))
                throw new ArgumentNullException (nameof (bankName));

            if (currencyId <= 0)
                throw new ArgumentNullException (nameof (currencyId));

                return new BankAccount
            {
                IBAN = iban.Trim().ToUpperInvariant(),
                BankName = bankName.Trim(),
                SwiftCode = swiftCode?.Trim().ToUpperInvariant(),
                CurrencyId = currencyId,
                AccountHolderName = accountHolderName?.Trim(),
                IsPrimary = isPrimary
            };
        }

        public void  Update(string iban, string bankName, string? swiftCode,
            int currencyId, string? accountHolderName, bool isPrimary)
        {
            if (string.IsNullOrWhiteSpace(iban))
                throw new ArgumentNullException(nameof(iban));

            if (string.IsNullOrWhiteSpace(bankName))
                throw new ArgumentNullException(nameof(bankName));

            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(currencyId);
            IBAN = iban.Trim().ToUpperInvariant();
            BankName = bankName.Trim();
            SwiftCode = swiftCode?.Trim().ToUpperInvariant();
            CurrencyId = currencyId;
            AccountHolderName = accountHolderName?.Trim();
            IsPrimary = isPrimary;
            

        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return IBAN;
            yield return CurrencyId;
        }
    }
}
