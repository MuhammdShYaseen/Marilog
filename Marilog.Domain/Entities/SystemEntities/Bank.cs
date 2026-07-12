using Marilog.Domain.Common;
using Marilog.Domain.ValueObjects.ReusableValueObjects;
using Marilog.Kernel.Primitives;
namespace Marilog.Domain.Entities.SystemEntities
{
    public class Bank : Entity
    {
        public string Name { get; private set; } = null!;
        public string? ShortName { get; private set; }
        public string? LegalName { get; private set; }

        /// <summary>
        /// Parent bank if this record represents a branch.
        /// Null means this record is the parent (head bank).
        /// </summary>
        public int? ParentBankId { get; private set; }

        public Bank? ParentBank { get; private set; }


        /// <summary>
        /// SWIFT/BIC code of this bank or branch.
        /// Usually assigned to the branch.
        /// </summary>
        public string? SwiftBic { get; private set; }

        public string? BranchCode { get; private set; }

        public string? ClearingCode { get; private set; }

        public string? NationalBankCode { get; private set; }

        public int CountryId { get; private set; }
        public Country Country { get; private set; } = null!;

        public string? City { get; private set; }

        public string? Address { get; private set; }

        public string? Website { get; private set; }

        public string? Notes { get; private set; }

        private readonly List<Bank> _branches = new();
        public IReadOnlyCollection<Bank> Branches => _branches.AsReadOnly();

        private readonly List<ContactEmail> _emails = new();
        public IReadOnlyCollection<ContactEmail> Emails => _emails.AsReadOnly();


        private readonly List<ContactPhone> _phones = new();
        public IReadOnlyCollection<ContactPhone> Phones => _phones.AsReadOnly();

        public ContactEmail? PrimaryEmail => _emails.FirstOrDefault(e => e.IsPrimary);
        public ContactPhone? PrimaryPhone => _phones.FirstOrDefault(p => p.IsPrimary);

        private Bank(){ } //EF

        public static Bank Create(string name, string shortName, string legalName,
                                  int? parentBankId, string swiftBic, string branchCode,
                                  string clearingCode, string nationalBankCode, int countryId,
                                  string city, string address, string website, string note)
        {
            

                return new Bank
            {
                Name = name,
                ShortName = shortName,
                LegalName = legalName,
                ParentBankId = parentBankId,
                SwiftBic = swiftBic,
                BranchCode = branchCode,
                ClearingCode = clearingCode,
                NationalBankCode = nationalBankCode,
                CountryId = countryId,
                City = city,
                Address = address,
                Website = website,
                Notes = note
            };
        }


        public void Update(string name, string shortName, string legalName,
                                  int? parentBankId, string swiftBic, string branchCode,
                                  string clearingCode, string nationalBankCode, int countryId,
                                  string city, string address, string website, string note)
        {
            if (parentBankId.HasValue == true && parentBankId.Value == Id) 
                    throw new InvalidOperationException("cannot bind it with it self");

            Name = name;
            ShortName = shortName;
            LegalName = legalName;
            ParentBankId = parentBankId;
            SwiftBic = swiftBic;
            BranchCode = branchCode;
            ClearingCode = clearingCode;
            NationalBankCode = nationalBankCode;
            CountryId = countryId;
            City = city;
            Address = address;
            Website = website;
            Notes = note;
            Touch();

        }

        // ── Emails ────────────────────────────────────────────────────
        public Result AddEmail(ContactEmail email)
        {
            if (_emails.Any(e => e.Address == email.Address))
                return Result.Fail("This email already exists.");

            if (email.IsPrimary)
                ClearPrimaryEmails();

            _emails.Add(email);
            Touch();
            return Result.Ok();
        }

        public Result RemoveEmail(string address)
        {
            var email = _emails.FirstOrDefault(e => e.Address == address);
            if (email == null)
                return Result.Fail("Email Address not found.");
            _emails.Remove(email);
            Touch();
            return Result.Ok();
        }

        // ── Phones ────────────────────────────────────────────────────
        public Result AddPhone(ContactPhone phone)
        {
            if (_phones.Any(p => p.Number == phone.Number && p.Type == phone.Type))
                return Result.Fail("This phone already exists.");

            if (phone.IsPrimary)
                ClearPrimaryPhones();

            _phones.Add(phone);
            Touch();
            return Result.Ok();
        }

        public Result RemovePhone(string phoneNumber)
        {
            var phone = _phones.FirstOrDefault(p => p.Number == phoneNumber);
            if (phone == null)
                return Result.Fail("Phone number not found");

            _phones.Remove(phone);
            Touch();
            return Result.Ok();
        }
        //----------HELPERS----------------------------------------------
        private void ClearPrimaryEmails()
        {
            var primary = _emails.FirstOrDefault(e => e.IsPrimary);
            primary?.Update(primary.Address, primary.Role, primary.Label!, isPrimary: false);
        }

        private void ClearPrimaryPhones()
        {
            var primary = _phones.FirstOrDefault(p => p.IsPrimary);
            primary?.Update(primary.Number, primary.Type, primary.Label, isPrimary: false);
        }


    }
}
