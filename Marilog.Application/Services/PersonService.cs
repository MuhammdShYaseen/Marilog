using Marilog.Application.DTOs.Responses;
using Marilog.Application.Interfaces.Services;
using Marilog.Domain.Entities;
using Marilog.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Marilog.Application.Services
{
    public class PersonService : IPersonService
    {
        private readonly IRepository<Person> _repo;

        public PersonService(IRepository<Person> repo) => _repo = repo;

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<PersonResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;

            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(ToResponse)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<PersonResponse?> GetByPassportAsync(string passportNo,
            CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;

            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.PassportNo == passportNo)
                .Select(ToResponse)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<PersonResponse?> GetBySeamanBookAsync(string seamanBookNo,
            CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .Where(x => x.SeamanBookNo == seamanBookNo)
                          .Select(ToResponse)
                          .FirstOrDefaultAsync(x => x.SeamanBookNo == seamanBookNo, ct);

        public async Task<IReadOnlyList<PersonResponse>> GetAllAsync(CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;

            return await _repo.Query()
                .AsNoTracking()
                .OrderBy(x => x.FullName)
                .Select(ToResponse)
                .ToListAsync(ct);
        }
        public async Task<IReadOnlyList<PersonResponse>> GetActiveAsync(CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;

            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.FullName)
                .Select(ToResponse)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<PersonResponse>> SearchAsync(string term,
            CancellationToken ct = default)
        {

            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.IsActive &&
                           (x.FullName.Contains(term) ||
                            x.PassportNo!.Contains(term) ||
                            x.SeamanBookNo!.Contains(term)))
                .OrderBy(x => x.FullName)
                .Select(ToResponse)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<PersonResponse>> GetWithExpiringPassportsAsync(int withinDays,
            CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            var threshold = DateOnly.FromDateTime(now.AddDays(withinDays));

            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.IsActive &&
                            x.PassportExpiry.HasValue &&
                            x.PassportExpiry.Value <= threshold)
                .OrderBy(x => x.PassportExpiry)
                .Select(ToResponse)
                .ToListAsync(ct);
        }

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<PersonResponse> CreateAsync(string bankName, string iBAN, bool isPassportExpired, string? bankSwiftCode,string fullName, int? nationality = null,
            string? passportNo = null, DateOnly? passportExpiry = null,
            string? seamanBookNo = null, DateOnly? dateOfBirth = null,
            string? phone = null, string? email = null,
            CancellationToken ct = default)
        {
            await EnsureUniqueDocumentsAsync(passportNo, seamanBookNo, excludeId: null, ct);

            var person = Person.Create(bankName, iBAN, isPassportExpired, bankSwiftCode, fullName, nationality, passportNo, passportExpiry,
                                       seamanBookNo, dateOfBirth, phone, email);
            await _repo.AddAsync(person, ct);
            await _repo.SaveChangesAsync(ct);
            return new PersonResponse
            {
                FullName = person.FullName,
                PassportExpiry = person.PassportExpiry,
                PassportNo = person.PassportNo,
                DateOfBirth = person.DateOfBirth,
                Phone = person.Phone,
                Email = person.Email,
                SeamanBookNo = person.SeamanBookNo,
                BankSwiftCode = person.BankSwiftCode,
                BankName = person.BankName,
                IBAN = person.IBAN,
                IsPassportExpired = person.IsPassportExpired(),
                Id = person.Id,
                IsActive = true
            };
        }

        public async Task UpdateAsync(int id, string fullName, int? nationality = null,
            string? passportNo = null, DateOnly? passportExpiry = null,
            string? seamanBookNo = null, DateOnly? dateOfBirth = null,
            string? phone = null, string? email = null,
            CancellationToken ct = default)
        {
            var person = await GetOrThrowAsync(id, ct);
            await EnsureUniqueDocumentsAsync(passportNo, seamanBookNo, excludeId: id, ct);

            person.Update(fullName, nationality, passportNo, passportExpiry,
                          seamanBookNo, dateOfBirth, phone, email);
            _repo.Update(person);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task ActivateAsync(int id, CancellationToken ct = default)
        {
            var person = await GetOrThrowAsync(id, ct);
            person.Activate();
            _repo.Update(person);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task DeactivateAsync(int id, CancellationToken ct = default)
        {
            var person = await GetOrThrowAsync(id, ct);
            person.Deactivate();
            _repo.Update(person);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var person = await GetOrThrowAsync(id, ct);
            _repo.HardDelete(person);
            await _repo.SaveChangesAsync(ct);
        }

        // ── Private ───────────────────────────────────────────────────────────────

        private async Task<Person> GetOrThrowAsync(int id, CancellationToken ct)
            => await _repo.GetByIdAsync(id, ct)
               ?? throw new KeyNotFoundException($"Person {id} not found.");

        private async Task EnsureUniqueDocumentsAsync(string? passportNo,
            string? seamanBookNo, int? excludeId, CancellationToken ct)
        {
            if (!string.IsNullOrWhiteSpace(passportNo))
            {
                var conflict = await _repo.Query()
                    .AnyAsync(x => x.PassportNo == passportNo &&
                                   (excludeId == null || x.Id != excludeId), ct);
                if (conflict)
                    throw new InvalidOperationException(
                        $"Passport number '{passportNo}' is already registered.");
            }

            if (!string.IsNullOrWhiteSpace(seamanBookNo))
            {
                var conflict = await _repo.Query()
                    .AnyAsync(x => x.SeamanBookNo == seamanBookNo &&
                                   (excludeId == null || x.Id != excludeId), ct);
                if (conflict)
                    throw new InvalidOperationException(
                        $"Seaman book number '{seamanBookNo}' is already registered.");
            }


        }

        private static readonly Expression<Func<Person, PersonResponse>> ToResponse =
            x => new PersonResponse
            {
                Id = x.Id,
                FullName = x.FullName,
                Nationality = x.NationalityCountry!.CountryName,
                NationalityCountryName = x.NationalityCountry != null
                    ? x.NationalityCountry.CountryName
                    : null,
                PassportNo = x.PassportNo!,
                PassportExpiry = x.PassportExpiry,

                // ⚠️ هذا سيتم ترجمته إلى SQL
                IsPassportExpired = x.PassportExpiry.HasValue &&
                            x.PassportExpiry < DateOnly.FromDateTime(DateTime.UtcNow),

                SeamanBookNo = x.SeamanBookNo!,
                DateOfBirth = x.DateOfBirth,
                Phone = x.Phone,
                Email = x.Email,
                BankName = x.BankName,
                IBAN = x.IBAN,
                BankSwiftCode = x.BankSwiftCode,
                IsActive = x.IsActive
            };

        // ── Bank Account ─────────────────────────────────────────────────────────

        public async Task UpdateBankAccountAsync(int id, string? bankName, string? iban,
            string? bankSwiftCode, CancellationToken ct = default)
        {
            var person = await GetOrThrowAsync(id, ct);
            person.UpdateBankAccount(bankName, iban, bankSwiftCode);
            _repo.Update(person);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task ClearBankAccountAsync(int id, CancellationToken ct = default)
        {
            var person = await GetOrThrowAsync(id, ct);
            person.ClearBankAccount();
            _repo.Update(person);
            await _repo.SaveChangesAsync(ct);
        }
    }
}
