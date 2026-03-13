using Marilog.Domain.Common;

namespace Marilog.Domain.Entities
{
    public class Person : Entity
    {
        public int PersonID { get; private set; }
        public string FullName { get; private set; } = null!;
        public int? Nationality { get; private set; }
        public Country? NationalityCountry { get; private set; }
        public string? PassportNo { get; private set; }
        public DateOnly? PassportExpiry { get; private set; }
        public string? SeamanBookNo { get; private set; }
        public DateOnly? DateOfBirth { get; private set; }
        public string? Phone { get; private set; }
        public string? Email { get; private set; }

        private readonly List<CrewContract> _contracts = new();
        public IReadOnlyCollection<CrewContract> Contracts => _contracts.AsReadOnly();


        public static Person Create(string fullName, int? nationality = null,
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
                Email = email
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

        public bool IsPassportExpired() =>
            PassportExpiry.HasValue && PassportExpiry.Value < DateOnly.FromDateTime(DateTime.UtcNow);
    }
}