using Marilog.Application.DTOs.Commands.Office;
using Marilog.Application.DTOs.Responses;
using Marilog.Application.Interfaces.Services;
using Marilog.Domain.Entities;
using Marilog.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Marilog.Application.Services
{
    public class OfficeService : IOfficeService
    {
        private readonly IRepository<Office> _repo;

        public OfficeService(IRepository<Office> repo) => _repo = repo;

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<OfficeResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(ToResponse)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<IReadOnlyList<OfficeResponse>> GetAllAsync(CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .OrderBy(x => x.OfficeName)
                .Select(ToResponse)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<OfficeResponse>> GetActiveAsync(CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.OfficeName)
                .Select(ToResponse)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<OfficeResponse>> GetByCountryAsync(int countryId,
            CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.CountryId == countryId && x.IsActive)
                .OrderBy(x => x.City)
                .ThenBy(x => x.OfficeName)
                .Select(ToResponse)
                .ToListAsync(ct);
        }

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<OfficeResponse> CreateAsync(string officeName, string city, int countryId,
            string? address = null, string? phone = null, string? contactName = null,
            CancellationToken ct = default)
        {
            var office = Office.Create(officeName, city, countryId, address, phone, contactName);
            await _repo.AddAsync(office, ct);
            await _repo.SaveChangesAsync(ct);
            return new OfficeResponse
            {
                Id = office.Id,
                Address = address,
                IsActive = true,
                CountryId = countryId,
                City = city,
                ContactName = contactName,
                Phone = phone,
                Name = officeName
            };
        }

        public async Task<IReadOnlyList<OfficeResponse>> CreateRangeAsync(
        IEnumerable<CreateOfficeCommand> commands,
        CancellationToken ct = default)
        {
            var offices = commands
                .Select(c => Office.Create(c.OfficeName, c.City, c.CountryId,
                                           c.Address, c.Phone, c.ContactName))
                .ToList();

            await _repo.AddRangeAsync(offices, ct);
            await _repo.SaveChangesAsync(ct);

            return offices
                .Select(office => new OfficeResponse
                {
                    Id = office.Id,
                    Name = office.OfficeName,
                    City = office.City,
                    CountryId = office.CountryId,
                    Address = office.Address,
                    Phone = office.Phone,
                    ContactName = office.ContactName,
                    IsActive = office.IsActive
                })
                .ToList();
        }

        public async Task UpdateAsync(int id, string officeName, string city, int countryId,
            string? address = null, string? phone = null, string? contactName = null,
            CancellationToken ct = default)
        {
            var office = await GetOrThrowAsync(id, ct);
            office.Update(officeName, city, countryId, address, phone, contactName);
            _repo.Update(office);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task ActivateAsync(int id, CancellationToken ct = default)
        {
            var office = await GetOrThrowAsync(id, ct);
            office.Activate();
            _repo.Update(office);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task DeactivateAsync(int id, CancellationToken ct = default)
        {
            var office = await GetOrThrowAsync(id, ct);
            office.Deactivate();
            _repo.Update(office);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var office = await GetOrThrowAsync(id, ct);
            _repo.HardDelete(office);
            await _repo.SaveChangesAsync(ct);
        }

        // ── Private ───────────────────────────────────────────────────────────────

        private async Task<Office> GetOrThrowAsync(int id, CancellationToken ct)
            => await _repo.GetByIdAsync(id, ct)
               ?? throw new KeyNotFoundException($"Office {id} not found.");

        private static readonly Expression<Func<Office, OfficeResponse>> ToResponse =
            x => new OfficeResponse
            {
                Id = x.Id,
                Name = x.OfficeName,
                City = x.City,
                CountryId = x.CountryId,
                CountryName = x.Country != null ? x.Country.CountryName : null,
                Address = x.Address,
                Phone = x.Phone,
                ContactName = x.ContactName,
                IsActive = x.IsActive
            };
    }
}
