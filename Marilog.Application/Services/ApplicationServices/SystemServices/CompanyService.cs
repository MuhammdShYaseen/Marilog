using Marilog.Contracts.DTOs.Requests.CompanyDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Domain.Entities.SystemEntities;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Domain.ValueObjects.ReusableValueObjects;
using Marilog.Kernel.Enums;
using Microsoft.EntityFrameworkCore;

namespace Marilog.Application.Services.ApplicationServices.SystemServices
{
    public class CompanyService : ICompanyService
    {
        private readonly IRepository<Company> _repo;

        public CompanyService(IRepository<Company> repo) => _repo = repo;

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<CompanyResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var companyDto = await _repo.Query()
               .AsNoTracking()
               .Select(x => new CompanyResponse
               {
                   Id = x.Id,
                   Address = x.Address,
                   IsActive = x.IsActive,
                   BankAccounts = x.BankAccounts.Select(b => new BankAccountResponse
                   {
                       IBAN = b.IBAN,
                       BankName = b.BankName,
                       SwiftCode = b.SwiftCode,
                       CurrencyId = b.CurrencyId,
                       AccountHolderName = b.AccountHolderName,
                       IsPrimary = b.IsPrimary
                   }).ToList(),

                   Emails = x.Emails.Select(e => new EmailsResponse
                   {
                       Address = e.Address,
                       Role = e.Role,
                       Label = e.Label,
                       IsPrimary = e.IsPrimary
                   }).ToList(),

                   Phones = x.Phones.Select(p => new PhonesResponse
                   {
                       Number = p.Number,
                       Type = p.Type,
                       Label = p.Label,
                       IsPrimary = p.IsPrimary
                   }).ToList(),
                   ContactName = x.ContactName,
                   RegistrationNumber = x.RegistrationNumber,
                   Name = x.CompanyName,
                   CountryId = x.CountryId,
               }).FirstOrDefaultAsync(c => c.Id == id, ct);
            return companyDto;
        }

        public async Task<CompanyResponse?> GetWithVesselsAsync(int id, CancellationToken ct = default)
        {
            var companyDto = await _repo.Query()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new CompanyResponse
                {
                    Id = x.Id,
                    Address = x.Address,
                    IsActive = x.IsActive,
                    BankAccounts = x.BankAccounts.Select(b => new BankAccountResponse
                    {
                        IBAN = b.IBAN,
                        BankName = b.BankName,
                        SwiftCode = b.SwiftCode,
                        CurrencyId = b.CurrencyId,
                        AccountHolderName = b.AccountHolderName,
                        IsPrimary = b.IsPrimary
                    }).ToList(),

                    Emails = x.Emails.Select(e => new EmailsResponse
                    {
                        Address = e.Address,
                        Role = e.Role,
                        Label = e.Label,
                        IsPrimary = e.IsPrimary
                    }).ToList(),

                    Phones = x.Phones.Select(p => new PhonesResponse
                    {
                        Number = p.Number,
                        Type = p.Type,
                        Label = p.Label,
                        IsPrimary = p.IsPrimary
                    }).ToList(),
                    ContactName = x.ContactName,
                    RegistrationNumber = x.RegistrationNumber,
                    Name = x.CompanyName,
                    Vessels = x.Vessels
                .Select(v => new VesselResponse
                {
                    Id = v.Id,
                    Name = v.VesselName,
                    IMONumber = v.IMONumber,
                    FlagCountryId = v.FlagCountryID,
                    FlagCountryName = v.FlagCountry!.CountryName,
                    IsActive = v.IsActive,
                    GrossTonnage = v.GrossTonnage
                })
                .ToList()
                })
                .FirstOrDefaultAsync(ct);

            return companyDto;
        }

        public async Task<IReadOnlyList<CompanyResponse>> GetAllAsync(CancellationToken ct = default)
        {
            var companies = await _repo.Query()
                .AsNoTracking()
                .OrderBy(x => x.CompanyName)   // ترتيب في SQL
                .Select(x => new CompanyResponse
                {
                    Id = x.Id,
                    Name = x.CompanyName,
                    Address = x.Address,
                    IsActive = x.IsActive,
                    BankAccounts = x.BankAccounts.Select(b => new BankAccountResponse
                    {
                        IBAN = b.IBAN,
                        BankName = b.BankName,
                        SwiftCode = b.SwiftCode,
                        CurrencyId = b.CurrencyId,
                        AccountHolderName = b.AccountHolderName,
                        IsPrimary = b.IsPrimary
                    }).ToList(),

                    Emails = x.Emails.Select(e => new EmailsResponse
                    {
                        Address = e.Address,
                        Role = e.Role,
                        Label = e.Label,
                        IsPrimary = e.IsPrimary
                    }).ToList(),

                    Phones = x.Phones.Select(p => new PhonesResponse
                    {
                        Number = p.Number,
                        Type = p.Type,
                        Label = p.Label,
                        IsPrimary = p.IsPrimary
                    }).ToList(),
                    ContactName = x.ContactName,
                    RegistrationNumber = x.RegistrationNumber
                })
                .ToListAsync(ct);

            return companies;
        }

        public async Task<IReadOnlyList<CompanyResponse>> GetActiveAsync(CancellationToken ct = default)
        {
            var companies = await _repo.Query()
                .AsNoTracking()
                .Where(x => x.IsActive)                // تصفية في SQL
                .OrderBy(x => x.CompanyName)           // ترتيب في SQL
                .Select(x => new CompanyResponse       // projection مباشر إلى DTO
                {
                    Id = x.Id,
                    Name = x.CompanyName,
                    Address = x.Address,
                    IsActive = x.IsActive,



                    BankAccounts = x.BankAccounts.Select(b => new BankAccountResponse
                    {
                        IBAN = b.IBAN,
                        BankName = b.BankName,
                        SwiftCode = b.SwiftCode,
                        CurrencyId = b.CurrencyId,
                        AccountHolderName = b.AccountHolderName,
                        IsPrimary = b.IsPrimary
                    }).ToList(),
                    Emails = x.Emails.Select(e => new EmailsResponse
                    {
                        Address = e.Address,
                        Role = e.Role,
                        Label = e.Label,
                        IsPrimary = e.IsPrimary
                    }).ToList(),
                    Phones = x.Phones.Select(p => new PhonesResponse
                    {
                        Number = p.Number,
                        Type = p.Type,
                        Label = p.Label,
                        IsPrimary = p.IsPrimary
                    }).ToList(),



                    ContactName = x.ContactName,
                    RegistrationNumber = x.RegistrationNumber
                })
                .ToListAsync(ct);

            return companies;
        }

        public async Task<IReadOnlyList<CompanyResponse>> SearchByNameAsync(string name, CancellationToken ct = default)
        {
            var companies = await _repo.Query()
                .AsNoTracking()
                .Where(x => x.IsActive && x.CompanyName.Contains(name)) // تصفية في SQL
                .OrderBy(x => x.CompanyName)                            // ترتيب في SQL
                .Select(x => new CompanyResponse                         // projection مباشر
                {
                    Id = x.Id,
                    Name = x.CompanyName,
                    Address = x.Address,
                    IsActive = x.IsActive,
                    BankAccounts = x.BankAccounts.Select(b => new BankAccountResponse
                    {
                        IBAN = b.IBAN,
                        BankName = b.BankName,
                        SwiftCode = b.SwiftCode,
                        CurrencyId = b.CurrencyId,
                        AccountHolderName = b.AccountHolderName,
                        IsPrimary = b.IsPrimary
                    }).ToList(),

                    Emails = x.Emails.Select(e => new EmailsResponse
                    {
                        Address = e.Address,
                        Role = e.Role,
                        Label = e.Label,
                        IsPrimary = e.IsPrimary
                    }).ToList(),

                    Phones = x.Phones.Select(p => new PhonesResponse
                    {
                        Number = p.Number,
                        Type = p.Type,
                        Label = p.Label,
                        IsPrimary = p.IsPrimary
                    }).ToList(),
                    ContactName = x.ContactName,
                    RegistrationNumber = x.RegistrationNumber
                })
                .ToListAsync(ct);

            return companies;
        }

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<CompanyResponse> CreateAsync(string? webSite, string? registrationNumber, string companyName, int? countryId = null, string? contactName = null, string? address = null,  CancellationToken ct = default)
        {
            var company = Company.Create(webSite, registrationNumber, companyName, countryId, contactName, address);
            await _repo.AddAsync(company, ct);
            await _repo.SaveChangesAsync(ct);
            return new CompanyResponse
            {
                Id = company.Id,
                Name = company.CompanyName,
                Address = company.Address,
                IsActive = company.IsActive,
                ContactName = company.ContactName,
                RegistrationNumber = company.RegistrationNumber,
                WebSite = company.Website
            };
        }

        public async Task<IReadOnlyList<CompanyResponse>> CreateRangeAsync(
        IEnumerable<CreateCompanyRequest> commands, CancellationToken ct = default)
        {
            var companies = commands
                .Select(c => Company.Create(
                    c.WebSite,
                    c.RegistrationNumber,
                    c.CompanyName,
                    c.CountryId,
                    c.ContactName,
                    c.Address))
                .ToList();

            await _repo.AddRangeAsync(companies, ct);
            await _repo.SaveChangesAsync(ct);

            return companies
                .Select(company => new CompanyResponse
                {
                    Id = company.Id,
                    Name = company.CompanyName,
                    Address = company.Address,
                    IsActive = company.IsActive,
                    
                    WebSite = company.Website,
                    ContactName = company.ContactName,
                    RegistrationNumber = company.RegistrationNumber
                })
                .ToList();
        }

        public async Task UpdateAsync(int id, string? registrationNumber,string? webSite, string companyName, int? countryId = null,
            string? contactName = null,
            string? address = null,
            CancellationToken ct = default)
        {
            var company = await GetOrThrowAsync(id, ct);
            company.Update(webSite, registrationNumber, companyName, countryId, contactName, address);
            _repo.Update(company);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task ActivateAsync(int id, CancellationToken ct = default)
        {
            var company = await GetOrThrowAsync(id, ct);
            company.Activate();
            _repo.Update(company);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task DeactivateAsync(int id, CancellationToken ct = default)
        {
            var company = await GetOrThrowAsync(id, ct);
            company.Deactivate();
            _repo.Update(company);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var company = await GetOrThrowAsync(id, ct);

            var hasVessels = await _repo.Query()
                .Where(x => x.Id == id)
                .SelectMany(x => x.Vessels)
                .AnyAsync(ct);
            if (hasVessels)
                throw new InvalidOperationException(
                    "Cannot delete a company that has vessels. Deactivate it instead.");

            _repo.HardDelete(company);
            await _repo.SaveChangesAsync(ct);
        }


        // ── Bank Accounts ─────────────────────────────────────────────────────────
        public async Task AddBankAccountAsync(int companyId, string iban, string bankName, string? swiftCode,
            int currencyId, string? accountHolderName, bool isPrimary, CancellationToken ct = default)
        {
            var company = await GetOrThrowAsync(companyId, ct);

            if (company == null)
                throw new KeyNotFoundException(nameof(company));

            var account = BankAccount.Create(iban, bankName, swiftCode, currencyId, accountHolderName, isPrimary);
            company.AddBankAccount(account);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task UpdateBankAccountAsync(int companyId, string iban, string bankName, string? swiftCode,
            int currencyId, string? accountHolderName, bool isPrimary, CancellationToken ct = default)
        {
            var company = await GetOrThrowAsync(companyId, ct);

            if (company == null)
                throw new KeyNotFoundException(nameof(company));


            var account = company.BankAccounts.FirstOrDefault(b => b.IBAN == iban.Trim().ToUpperInvariant())
                ?? throw new KeyNotFoundException($"Bank account {iban} not found.");


            account.Update(iban, bankName, swiftCode, currencyId, accountHolderName, isPrimary);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task RemoveBankAccountAsync(int companyId, string iban, CancellationToken ct = default)
        {
            var company = await GetOrThrowAsync(companyId, ct);

            if (company == null)
                throw new KeyNotFoundException(nameof(company));
            company.RemoveBankAccount(iban);
            await _repo.SaveChangesAsync(ct);
        }

        // ── Emails ────────────────────────────────────────────────────────────────
        public async Task AddEmailAsync(int companyId, string address, EmailRole role, string? label, bool isPrimary,
            CancellationToken ct = default)
        {
            var company = await GetOrThrowAsync(companyId, ct);

            if (company == null)
                throw new KeyNotFoundException(nameof(company));
            var email = ContactEmail.Create(address, role, label!, isPrimary);
            company.AddEmail(email);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task UpdateEmailAsync(int companyId, string oldAddress, string newAddress, EmailRole role,
            string? label, bool isPrimary, CancellationToken ct = default)
        {
            var company = await GetOrThrowAsync(companyId, ct);

            if (company == null)
                throw new KeyNotFoundException(nameof(company));

            var email = company.Emails.FirstOrDefault(e => e.Address == oldAddress.Trim().ToLowerInvariant())
                ?? throw new KeyNotFoundException($"Email {oldAddress} not found.");
            email.Update(newAddress, role, label!, isPrimary);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task RemoveEmailAsync(int companyId, string address, CancellationToken ct = default)
        {
            var company = await GetOrThrowAsync(companyId, ct);

            if (company == null)
                throw new KeyNotFoundException(nameof(company));

            var email = company.Emails.FirstOrDefault(e => e.Address == address.Trim().ToLowerInvariant())
                ?? throw new KeyNotFoundException($"Email {address} not found.");

            company.RemoveEmail(address);
            await _repo.SaveChangesAsync(ct);
        }

        // ── Phones ────────────────────────────────────────────────────────────────
        public async Task AddPhoneAsync(int companyId, string number, PhoneType type, string? label, bool isPrimary,
            CancellationToken ct = default)
        {
            var company = await GetOrThrowAsync(companyId, ct);

            if (company == null)
                throw new KeyNotFoundException(nameof(company));

            var phone = ContactPhone.Create(number, type, label, isPrimary);
            company.AddPhone(phone);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task UpdatePhoneAsync(int companyId, string oldNumber, PhoneType oldType, string newNumber,
            PhoneType newType, string? label, bool isPrimary, CancellationToken ct = default)
        {
            var company = await GetOrThrowAsync(companyId, ct);

            if (company == null)
                throw new KeyNotFoundException(nameof(company));

            var phone = company.Phones.FirstOrDefault(p => p.Number == oldNumber && p.Type == oldType)
                ?? throw new KeyNotFoundException($"Phone {oldNumber} not found.");
            phone.Update(newNumber, newType, label, isPrimary);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task RemovePhoneAsync(int companyId, string number, PhoneType type, CancellationToken ct = default)
        {
            var company = await GetOrThrowAsync(companyId, ct);

            if (company == null)
                throw new KeyNotFoundException(nameof(company));
            var phone = company.Phones.FirstOrDefault(p => p.Number == number && p.Type == type)
                ?? throw new KeyNotFoundException($"Phone {number} not found.");
            company.RemovePhone(number);
            await _repo.SaveChangesAsync(ct);
        }

        // ── Private ───────────────────────────────────────────────────────────────

        private async Task<Company> GetOrThrowAsync(int id, CancellationToken ct)
            => await _repo.Query()
                .Include(p => p.Phones)
                .Include(p => p.Emails)
                .Include(b => b.BankAccounts)
                .Where(a => a.Id == id)
                .FirstOrDefaultAsync(ct)
               ?? throw new KeyNotFoundException($"Company {id} not found.");
    }
}
