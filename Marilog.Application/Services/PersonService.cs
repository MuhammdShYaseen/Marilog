using Microsoft.EntityFrameworkCore;
using Marilog.Domain.Entities;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Domain.Interfaces.Services;

namespace Marilog.Application.Services
{
    public class PersonService : IPersonService
    {
        private readonly IRepository<Person> _repo;

        public PersonService(IRepository<Person> repo) => _repo = repo;

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<Person?> GetByIdAsync(int id, CancellationToken ct = default)
            => await _repo.GetByIdAsync(id, ct);

        public async Task<Person?> GetByPassportAsync(string passportNo,
            CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .FirstOrDefaultAsync(x => x.PassportNo == passportNo, ct);

        public async Task<Person?> GetBySeamanBookAsync(string seamanBookNo,
            CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .FirstOrDefaultAsync(x => x.SeamanBookNo == seamanBookNo, ct);

        public async Task<IReadOnlyList<Person>> GetAllAsync(CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .OrderBy(x => x.FullName)
                          .ToListAsync(ct);

        public async Task<IReadOnlyList<Person>> GetActiveAsync(CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .Where(x => x.IsActive)
                          .OrderBy(x => x.FullName)
                          .ToListAsync(ct);

        public async Task<IReadOnlyList<Person>> SearchAsync(string term,
            CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .Where(x => x.IsActive && (
                              x.FullName.Contains(term)     ||
                              x.PassportNo!.Contains(term)  ||
                              x.SeamanBookNo!.Contains(term)))
                          .OrderBy(x => x.FullName)
                          .ToListAsync(ct);

        public async Task<IReadOnlyList<Person>> GetWithExpiringPassportsAsync(int withinDays,
            CancellationToken ct = default)
        {
            var threshold = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(withinDays));
            return await _repo.Query().AsNoTracking()
                              .Where(x => x.IsActive                      &&
                                          x.PassportExpiry.HasValue        &&
                                          x.PassportExpiry.Value <= threshold)
                              .OrderBy(x => x.PassportExpiry)
                              .ToListAsync(ct);
        }

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<Person> CreateAsync(string fullName, int? nationality = null,
            string? passportNo = null, DateOnly? passportExpiry = null,
            string? seamanBookNo = null, DateOnly? dateOfBirth = null,
            string? phone = null, string? email = null,
            CancellationToken ct = default)
        {
            await EnsureUniqueDocumentsAsync(passportNo, seamanBookNo, excludeId: null, ct);

            var person = Person.Create(fullName, nationality, passportNo, passportExpiry,
                                       seamanBookNo, dateOfBirth, phone, email);
            await _repo.AddAsync(person, ct);
            await _repo.SaveChangesAsync(ct);
            return person;
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
                                   (excludeId == null || x.PersonID != excludeId), ct);
                if (conflict)
                    throw new InvalidOperationException(
                        $"Passport number '{passportNo}' is already registered.");
            }

            if (!string.IsNullOrWhiteSpace(seamanBookNo))
            {
                var conflict = await _repo.Query()
                    .AnyAsync(x => x.SeamanBookNo == seamanBookNo &&
                                   (excludeId == null || x.PersonID != excludeId), ct);
                if (conflict)
                    throw new InvalidOperationException(
                        $"Seaman book number '{seamanBookNo}' is already registered.");
            }


        }

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
