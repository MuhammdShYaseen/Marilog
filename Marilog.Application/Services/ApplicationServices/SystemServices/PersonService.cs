using Marilog.Contracts.DTOs.Requests.Common;
using Marilog.Contracts.DTOs.Requests.PersonDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Domain.Entities.SystemEntities;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Domain.ValueObjects.Person;
using Microsoft.EntityFrameworkCore;

namespace Marilog.Application.Services.ApplicationServices.SystemServices
{
    public class PersonService : IPersonService
    {
        private readonly IRepository<Person> _repo;
        private readonly IRepository<Rank> _rankRepo;

        public PersonService(IRepository<Person> repo, IRepository<Rank> rankRepo)
        {
            _repo = repo;
            _rankRepo = rankRepo;
        }

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<PersonResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var person = await _repo.Query()
                .AsNoTracking()
                .Include(p => p.NationalityCountry)
                .FirstOrDefaultAsync(p => p.Id == id, ct);

            if (person is null) return null;
            var ranks = await GetRanksDictAsync(person.SeaServices, ct);
            return BuildResponse(person, ranks);
        }

        public async Task<PersonResponse?> GetByPassportAsync(string passportNo,
            CancellationToken ct = default)
        {
            var person = await _repo.Query()
                .AsNoTracking()
                .Include(p => p.NationalityCountry)
                .FirstOrDefaultAsync(p => p.PassportNo == passportNo, ct);

            if (person is null) return null;
            var ranks = await GetRanksDictAsync(person.SeaServices, ct);
            return BuildResponse(person, ranks);
        }

        public async Task<PersonResponse?> GetBySeamanBookAsync(string seamanBookNo,
            CancellationToken ct = default)
        {
            var person = await _repo.Query()
                .AsNoTracking()
                .Include(p => p.NationalityCountry)
                .FirstOrDefaultAsync(p => p.SeamanBookNo == seamanBookNo, ct);

            if (person is null) return null;
            var ranks = await GetRanksDictAsync(person.SeaServices, ct);
            return BuildResponse(person, ranks);
        }

        public async Task<IReadOnlyList<PersonResponse>> GetAllAsync(CancellationToken ct = default)
            => await MapListAsync(_repo.Query().OrderBy(x => x.FullName), ct);

        public async Task<IReadOnlyList<PersonResponse>> GetActiveAsync(CancellationToken ct = default)
            => await MapListAsync(_repo.Query().Where(x => x.IsActive).OrderBy(x => x.FullName), ct);

        public async Task<IReadOnlyList<PersonResponse>> SearchAsync(string term,
            CancellationToken ct = default)
            => await MapListAsync(
                _repo.Query()
                    .Where(x => x.IsActive &&
                               (x.FullName.Contains(term) ||
                                x.PassportNo!.Contains(term) ||
                                x.SeamanBookNo!.Contains(term)))
                    .OrderBy(x => x.FullName), ct);

        public async Task<IReadOnlyList<PersonResponse>> GetWithExpiringPassportsAsync(int withinDays,
            CancellationToken ct = default)
        {
            var threshold = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(withinDays));
            return await MapListAsync(
                _repo.Query()
                    .Where(x => x.IsActive &&
                                x.PassportExpiry.HasValue &&
                                x.PassportExpiry.Value <= threshold)
                    .OrderBy(x => x.PassportExpiry), ct);
        }

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<PersonResponse> CreateAsync(string? bankName, string? iBAN,
            bool isPassportExpired, string? bankSwiftCode, string fullName,
            int? nationality = null, string? passportNo = null,
            DateOnly? passportExpiry = null, string? seamanBookNo = null,
            DateOnly? dateOfBirth = null, string? phone = null,
            string? email = null, CancellationToken ct = default)
        {
            await EnsureUniqueDocumentsAsync(passportNo, seamanBookNo, excludeId: null, ct);

            var person = Person.Create(bankName, iBAN, isPassportExpired, bankSwiftCode,
                fullName, nationality, passportNo, passportExpiry,
                seamanBookNo, dateOfBirth, phone, email);

            await _repo.AddAsync(person, ct);
            await _repo.SaveChangesAsync(ct);
            return BuildResponse(person, []);
        }

        public async Task<IReadOnlyList<PersonResponse>> CreateRangeAsync(
            IEnumerable<CreatePersonRequest> commands, CancellationToken ct = default)
        {
            if (commands == null || !commands.Any())
                return Array.Empty<PersonResponse>();

            var passportSet = new HashSet<string>();
            var seamanBookSet = new HashSet<string>();

            foreach (var c in commands)
            {
                if (!string.IsNullOrWhiteSpace(c.PassportNo) && !passportSet.Add(c.PassportNo))
                    throw new InvalidOperationException($"Duplicate PassportNo in request: {c.PassportNo}");

                if (!string.IsNullOrWhiteSpace(c.SeamanBookNo) && !seamanBookSet.Add(c.SeamanBookNo))
                    throw new InvalidOperationException($"Duplicate SeamanBookNo in request: {c.SeamanBookNo}");
            }

            var passportNos = passportSet.ToList();
            var seamanBookNos = seamanBookSet.ToList();

            var existing = await _repo.Query()
                .Where(p => (p.PassportNo != null && passportNos.Contains(p.PassportNo)) ||
                            (p.SeamanBookNo != null && seamanBookNos.Contains(p.SeamanBookNo)))
                .Select(p => new { p.PassportNo, p.SeamanBookNo })
                .ToListAsync(ct);

            if (existing.Any())
            {
                var existingPassports = existing.Where(p => p.PassportNo != null).Select(p => p.PassportNo);
                var existingSeaman = existing.Where(p => p.SeamanBookNo != null).Select(p => p.SeamanBookNo);

                if (existingPassports.Any())
                    throw new InvalidOperationException($"PassportNo(s) already exist: {string.Join(", ", existingPassports)}");
                if (existingSeaman.Any())
                    throw new InvalidOperationException($"SeamanBookNo(s) already exist: {string.Join(", ", existingSeaman)}");
            }

            var persons = commands.Select(c => Person.Create(
                c.BankName, c.IBAN, c.IsPassportExpired, c.BankSwiftCode,
                c.FullName, c.Nationality, c.PassportNo, c.PassportExpiry,
                c.SeamanBookNo, c.DateOfBirth, c.Phone, c.Email)).ToList();

            await _repo.AddRangeAsync(persons, ct);
            await _repo.SaveChangesAsync(ct);

            return persons.Select(p => BuildResponse(p, [])).ToList();
        }

        public async Task UpdateAsync(int id, string fullName, int? nationality = null,
            string? passportNo = null, DateOnly? passportExpiry = null,
            string? seamanBookNo = null, DateOnly? dateOfBirth = null,
            string? phone = null, string? email = null, CancellationToken ct = default)
        {
            var person = await GetOrThrowAsync(id, ct);
            await EnsureUniqueDocumentsAsync(passportNo, seamanBookNo, excludeId: id, ct);
            person.Update(fullName, nationality, passportNo, passportExpiry, seamanBookNo, dateOfBirth, phone, email);
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

        // ── Bank Account ──────────────────────────────────────────────────────────

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

        // ── Certificates ──────────────────────────────────────────────────────────

        public async Task AddCertificateAsync(int personId,
            UpsertCertificateRequest req, CancellationToken ct = default)
        {
            var person = await GetOrThrowAsync(personId, ct);
            person.AddCertificate(req.CertificateName, req.CertificateNumber, req.IssuingAuthority, req.IssueDate, req.ExpiryDate, req.Description);
            _repo.Update(person);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task UpdateCertificateAsync(int personId, int index,
            UpsertCertificateRequest req, CancellationToken ct = default)
        {
            var person = await GetOrThrowAsync(personId, ct);
            person.UpdateCertificate(index, req.CertificateName, req.CertificateNumber, req.IssuingAuthority, req.IssueDate, req.ExpiryDate, req.Description);
            _repo.Update(person);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task RemoveCertificateAsync(int personId, int index, CancellationToken ct = default)
        {
            var person = await GetOrThrowAsync(personId, ct);
            person.RemoveCertificate(index);
            _repo.Update(person);
            await _repo.SaveChangesAsync(ct);
        }

        // ── Sea Services ──────────────────────────────────────────────────────────

        public async Task AddSeaServiceAsync(int personId,
            UpsertSeaServiceRequest req, CancellationToken ct = default)
        {
            var person = await GetOrThrowAsync(personId, ct);
            person.AddSeaService(req.RankId, req.ExperienceInMonths, req.VesselSizeInMT);
            _repo.Update(person);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task UpdateSeaServiceAsync(int personId, int index,
            UpsertSeaServiceRequest req, CancellationToken ct = default)
        {
            var person = await GetOrThrowAsync(personId, ct);
            person.UpdateSeaService(index, req.RankId, req.ExperienceInMonths, req.VesselSizeInMT);
            _repo.Update(person);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task RemoveSeaServiceAsync(int personId, int index, CancellationToken ct = default)
        {
            var person = await GetOrThrowAsync(personId, ct);
            person.RemoveSeaService(index);
            _repo.Update(person);
            await _repo.SaveChangesAsync(ct);
        }

        // ── Private Helpers ───────────────────────────────────────────────────────

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
                    throw new InvalidOperationException($"Passport number '{passportNo}' is already registered.");
            }

            if (!string.IsNullOrWhiteSpace(seamanBookNo))
            {
                var conflict = await _repo.Query()
                    .AnyAsync(x => x.SeamanBookNo == seamanBookNo &&
                                   (excludeId == null || x.Id != excludeId), ct);
                if (conflict)
                    throw new InvalidOperationException($"Seaman book number '{seamanBookNo}' is already registered.");
            }
        }

        // ✅ ToList أولاً — ثم Map في الميموري بعيداً عن SQL
        private async Task<IReadOnlyList<PersonResponse>> MapListAsync(
            IQueryable<Person> query, CancellationToken ct)
        {
            var persons = await query
                .AsNoTracking()
                .Include(p => p.NationalityCountry)
                .Include(p => p.Certificates)   // ← أضف هذا
                .Include(p => p.SeaServices)    // ← وهذا
                .ToListAsync(ct);

            var ranks = await GetRanksDictAsync(
                persons.SelectMany(p => p.SeaServices), ct);

            return persons.Select(p => BuildResponse(p, ranks)).ToList();
        }

        private async Task<Dictionary<int, string>> GetRanksDictAsync(
            IEnumerable<PersonSeaService> seaServices, CancellationToken ct)
        {
            var rankIds = seaServices.Select(s => s.RankId).Distinct().ToList();
            if (!rankIds.Any()) return [];

            return await _rankRepo.Query()
                .Where(r => rankIds.Contains(r.Id))
                .ToDictionaryAsync(r => r.Id, r => r.RankName, ct);
        }

        private static PersonResponse BuildResponse(Person p, Dictionary<int, string> ranks) =>
            new()
            {
                Id = p.Id,
                FullName = p.FullName,
                Nationality = p.Nationality,
                NationalityCountryName = p.NationalityCountry?.CountryName,
                PassportNo = p.PassportNo,
                PassportExpiry = p.PassportExpiry,
                IsPassportExpired = p.IsPassportExpired(),
                SeamanBookNo = p.SeamanBookNo,
                DateOfBirth = p.DateOfBirth,
                Phone = p.Phone,
                Email = p.Email,
                BankName = p.BankName,
                IBAN = p.IBAN,
                BankSwiftCode = p.BankSwiftCode,
                IsActive = p.IsActive,

                // ✅ يُحسب في الميموري — مش في SQL
                Certificates = p.Certificates
                    .Select((c, i) => new PersonCertificateResponse
                    {
                        Index = i,
                        CertificateName = c.CertificateName,
                        IssueDate = c.IssueDate,
                        ExpiryDate = c.ExpiryDate,
                        Description = c.Description
                    }).ToList(),

                SeaServices = p.SeaServices
                    .Select((s, i) => new PersonSeaServiceResponse
                    {
                        Index = i,
                        RankId = s.RankId,
                        RankName = ranks.GetValueOrDefault(s.RankId),
                        ExperienceInMonths = s.ExperienceInMonths,
                        VesselSizeInMT = s.VesselSizeInMT
                    }).ToList()
            };
    }
}