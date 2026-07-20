using Marilog.Domain.Common;
using Marilog.Domain.Entities.Certificates;
using Marilog.Domain.ValueObjects.Person;
using Marilog.Domain.ValueObjects.ReusableValueObjects;
using Marilog.Kernel.Enums;

namespace Marilog.Domain.Entities.SystemEntities
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

        private readonly List<PersonCertificate> _certificates = new();
        public IReadOnlyCollection<PersonCertificate> Certificates => _certificates.AsReadOnly();

        private readonly List<PersonSeaService> _seaServices = new();
        public IReadOnlyCollection<PersonSeaService> SeaServices => _seaServices.AsReadOnly();

        private Person() { }
        public static Person Create(string? bankName, string? iBAN, bool isPassportExpired, string? bankSwiftCode, string fullName, int? nationality = null,
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

        //-----Certificates--------------------------
        public void AddCertificate(PersonCertificateType type, string name, string? certificateNumber,
            string? issuingAuthority, DateOnly? issueDate, DateOnly? expiryDate, string? description)
        {
            var certificate = Certificate.Create(name, certificateNumber, issuingAuthority, issueDate, expiryDate, description);
            _certificates.Add(PersonCertificate.Create(type, certificate));
            Touch();
        }

        public void UpdateCertificate(int certificateId, PersonCertificateType type, string name,
            string? certificateNumber, string? issuingAuthority, DateOnly? issueDate, DateOnly? expiryDate, string? description)
        {
            var existing = _certificates.FirstOrDefault(c => c.Id == certificateId)
                ?? throw new InvalidOperationException("Certificate not found.");

            var certificate = Certificate.Create(name, certificateNumber, issuingAuthority, issueDate, expiryDate, description);
            existing.Update(type, certificate);
            Touch();
        }

        public void RemoveCertificate(int certificateId)
        {
            var existing = _certificates.FirstOrDefault(c => c.Id == certificateId)
                ?? throw new InvalidOperationException("Certificate not found.");
            _certificates.Remove(existing);
            Touch();
        }


        //----Sea Services-----------------------------------
        public void UpdateSeaService(int index, int rankId, int experienceInMonths, decimal? vesselSizeInMT)
        {
            if (index < 0 || index >= _seaServices.Count)
                throw new IndexOutOfRangeException("Sea service not found.");

            _seaServices[index] = _seaServices[index]
                .WithUpdates(rankId, experienceInMonths, vesselSizeInMT);
            Touch();
        }
        public void AddSeaService(int rankId, int experienceInMonths, decimal? vesselSizeInMT)
        {
            _seaServices.Add(PersonSeaService.Create(rankId, experienceInMonths, vesselSizeInMT));
            Touch();
        }

        public void RemoveSeaService(int index)
        {
            if (index < 0 || index >= _seaServices.Count)
                throw new IndexOutOfRangeException("Sea service not found.");
            _seaServices.RemoveAt(index);
            Touch();
        }
    }
}