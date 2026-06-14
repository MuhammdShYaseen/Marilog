using Marilog.Domain.Common;
using Marilog.Domain.ValueObjects.ReusableValueObjects;
using Marilog.Kernel.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Domain.Entities.SystemEntities
{
    public class Company : Entity
    {
        public string CompanyName { get; private set; } = null!;
        public int? CountryId { get; private set; }
        public Country? Country { get; private set; }
        public string? ContactName { get; private set; }
        public string? Address { get; private set; }
        public string? Website { get; private set; }
        public string? RegistrationNumber { get; private set; }


        // ── Collections (Owned) ───────────────────────────────────────
        private readonly List<BankAccount> _bankAccounts = new();
        public IReadOnlyCollection<BankAccount> BankAccounts => _bankAccounts.AsReadOnly();

        private readonly List<ContactEmail> _emails = new();
        public IReadOnlyCollection<ContactEmail> Emails => _emails.AsReadOnly();

        private readonly List<ContactPhone> _phones = new();
        public IReadOnlyCollection<ContactPhone> Phones => _phones.AsReadOnly();
        private readonly List<Vessel> _vessels = new();
        public IReadOnlyCollection<Vessel> Vessels => _vessels.AsReadOnly();

        // ── Convenience accessors ─────────────────────────────────────
        public BankAccount? PrimaryBankAccount => _bankAccounts.FirstOrDefault(b => b.IsPrimary);
        public ContactEmail? PrimaryEmail => _emails.FirstOrDefault(e => e.IsPrimary);
        public ContactPhone? PrimaryPhone => _phones.FirstOrDefault(p => p.IsPrimary);

        private Company() { }
        public static Company Create(string? webSite, string? registrationNumber, string companyName, int? countryId,
            string? contactName = null, string? address = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(companyName);

            return new Company
            {
                RegistrationNumber = registrationNumber,
                CompanyName = companyName,
                CountryId = countryId,
                ContactName = contactName,
                Address = address,
                Website = webSite
            };
        }

        public void Update(string? webSite, string? registrationNumber, string companyName, int? countryId,
            string? contactName = null, string? address = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(companyName);

            CompanyName = companyName;
            CountryId = countryId;
            ContactName = contactName;
            Address = address;
            RegistrationNumber = registrationNumber;
            Website = webSite;
            Touch();
        }

        // ── Bank Accounts ─────────────────────────────────────────────
        public Result AddBankAccount(BankAccount account)
        {
            if (_bankAccounts.Any(b => b.IBAN == account.IBAN))
                return Result.Fail("This IBAN already exists.");

            if (account.IsPrimary)
                ClearPrimaryBankAccounts();

            _bankAccounts.Add(account);
            return Result.Ok();
        }

        public Result RemoveBankAccount(string iban)
        {
            var account = _bankAccounts.FirstOrDefault(b => b.IBAN == iban);
            if (account is null)
                return Result.Fail("Bank account not found.");

            _bankAccounts.Remove(account);
            return Result.Ok();
        }

        // ── Emails ────────────────────────────────────────────────────
        public Result AddEmail(ContactEmail email)
        {
            if (_emails.Any(e => e.Address == email.Address))
                return Result.Fail("This email already exists.");

            if (email.IsPrimary)
                ClearPrimaryEmails();

            _emails.Add(email);
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
            return Result.Ok();
        }

        // ── Private helpers ───────────────────────────────────────────
        private void ClearPrimaryBankAccounts()
        {
            var primary = _bankAccounts.FirstOrDefault(b => b.IsPrimary);
            primary?.Update(primary.IBAN, primary.BankName, primary.SwiftCode,
                primary.CurrencyId, primary.AccountHolderName, isPrimary: false);
        }

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
