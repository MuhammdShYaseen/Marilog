using Marilog.Domain.Common;

namespace Marilog.Domain.Entities
{
    public class Person : Entity
    {
        public string FullName { get; private set; } = null!;
        public int? Nationality { get; private set; }
        public Country? NationalityCountry { get; private set; }
        public string? PassportNo { get; private set; }
        public DateOnly? PassportExpiry { get; private set; }
        public string? SeamanBookNo { get; private set; }
        public DateOnly? DateOfBirth { get; private set; }
        public string? Phone { get; private set; }
        public string? Email { get; private set; }
        public string? BankName { get; private set; }
        public string? IBAN { get; private set; }
        public string? BankSwiftCode { get; private set; }

        private readonly List<CrewContract> _contracts = new();
        public IReadOnlyCollection<CrewContract> Contracts => _contracts.AsReadOnly();

        private Person() { }
        public static Person Create(string bankName, string iBAN, bool isPassportExpired, string? bankSwiftCode, string fullName, int? nationality = null,
            string? passportNo = null, DateOnly? passportExpiry = null,
            string? seamanBookNo = null, DateOnly? dateOfBirth = null,
            string? phone = null, string? email = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(fullName);

            return new Person
            {
                FullName = fullName,
                Nationality = nationality,
                PassportNo = passportNo,
                PassportExpiry = passportExpiry,
                SeamanBookNo = seamanBookNo,
                DateOfBirth = dateOfBirth,
                Phone = phone,
                Email = email,
                IBAN = iBAN,
                BankName = bankName,
                BankSwiftCode = bankSwiftCode
            };
        }

        public void Update(string fullName, int? nationality = null,
            string? passportNo = null, DateOnly? passportExpiry = null,
            string? seamanBookNo = null, DateOnly? dateOfBirth = null,
            string? phone = null, string? email = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(fullName);

            FullName = fullName;
            Nationality = nationality;
            PassportNo = passportNo;
            PassportExpiry = passportExpiry;
            SeamanBookNo = seamanBookNo;
            DateOfBirth = dateOfBirth;
            Phone = phone;
            Email = email;
            Touch();
        }
        public void UpdateBankAccount(string? bankName, string? iban, string? swiftCode)
        {
            BankName = bankName;
            IBAN = iban;
            BankSwiftCode = swiftCode;
            Touch();
        }
        public bool IsPassportExpired() =>
            PassportExpiry.HasValue && PassportExpiry.Value < DateOnly.FromDateTime(DateTime.UtcNow);

        public void ClearBankAccount()
        {
            BankName = null;
            IBAN = null;
            BankSwiftCode = null;
            Touch();
        }
    }
}