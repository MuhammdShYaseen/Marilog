using Microsoft.EntityFrameworkCore;
using Marilog.Domain.Entities;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Application.Interfaces.Services;
using Marilog.Application.DTOs;

namespace Marilog.Application.Services
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
                   Email = x.Email,
                   Phone = x.Phone,
                   ContactName = x.ContactName,
                   RegistrationNumber = x.RegistrationNumber,
                   Name = x.CompanyName
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
                    Email = x.Email,
                    Phone = x.Phone,
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
                    Email = x.Email,
                    Phone = x.Phone,
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
                    Email = x.Email,
                    Phone = x.Phone,
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
                    Email = x.Email,
                    Phone = x.Phone,
                    ContactName = x.ContactName,
                    RegistrationNumber = x.RegistrationNumber
                })
                .ToListAsync(ct);

            return companies;
        }

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<CompanyResponse> CreateAsync(string? registrationNumber, string companyName, int? countryId = null, string? contactName = null, string? email = null, string? phone = null, string? address = null,  CancellationToken ct = default)
        {
            var company = Company.Create(registrationNumber, companyName, countryId, contactName, email, phone, address);
            await _repo.AddAsync(company, ct);
            await _repo.SaveChangesAsync(ct);
            return new CompanyResponse
            {
                Id = company.Id,
                Name = company.CompanyName,
                Address = company.Address,
                IsActive = company.IsActive,
                Email = company.Email,
                Phone = company.Phone,
                ContactName = company.ContactName,
                RegistrationNumber = company.RegistrationNumber
            };
        }

        public async Task UpdateAsync(int id, string companyName, int? countryId = null,
            string? contactName = null, string? email = null,
            string? phone = null, string? address = null,
            CancellationToken ct = default)
        {
            var company = await GetOrThrowAsync(id, ct);
            company.Update(companyName, countryId, contactName, email, phone, address);
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

        // ── Private ───────────────────────────────────────────────────────────────

        private async Task<Company> GetOrThrowAsync(int id, CancellationToken ct)
            => await _repo.GetByIdAsync(id, ct)
               ?? throw new KeyNotFoundException($"Company {id} not found.");
    }
}
