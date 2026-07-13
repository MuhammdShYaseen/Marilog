using Marilog.Contracts.Common;                // PagedResponse<T>, LookupItem<T>
using Marilog.Contracts.DTOs.Requests.BankDTOs;
using Marilog.Contracts.DTOs.Requests.ContactsRequestDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Domain.Entities.SystemEntities;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Domain.ValueObjects.ReusableValueObjects;
using Marilog.Kernel.Primitives;
using Microsoft.EntityFrameworkCore;

namespace Marilog.Application.Services.Banks
{
    public class BankService : IBankService
    {
        private readonly IRepository<Bank> _repo;

        public BankService(IRepository<Bank> repo)
        {
            _repo = repo;
        }

        // ── Query ─────────────────────────────────────────────────
        public async Task<BankResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var bank = await _repo.Query()
                .AsNoTracking()
                .Include(b => b.Country)
                .Include(b => b.Branches)
                .Include(b => b.ParentBank)
                .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

            return bank is null ? null : Map(bank, includeBranches: true);
        }

        public async Task<PagedResponse<BankResponse>> GetPagedAsync(
            bool treeMode = false, CancellationToken cancellationToken = default)
        {
            var query = _repo.Query()
                .AsNoTracking()
                .Include(b => b.Country)
                .Include(b => b.Branches)
                .AsQueryable();

            if (treeMode)
                query = query.Where(b => b.ParentBankId == null);

            var banks = await query.OrderBy(b => b.Name).ToListAsync(cancellationToken);
            var items = banks.Select(b => Map(b, includeBranches: treeMode)).ToList();

            return new PagedResponse<BankResponse>
            {
                Items = items,
                TotalCount = items.Count,
                Page = 1,
                PageSize = items.Count
            };
        }

        public async Task<IReadOnlyList<BankResponse>> GetAllActiveAsync(
            bool treeMode = false, CancellationToken cancellationToken = default)
        {
            var query = _repo.Query()
                .AsNoTracking()
                .Include(b => b.Country)
                .Include(b => b.Branches)
                .Where(b => b.IsActive);

            if (treeMode)
                query = query.Where(b => b.ParentBankId == null);

            var banks = await query.OrderBy(b => b.Name).ToListAsync(cancellationToken);
            return banks.Select(b => Map(b, includeBranches: treeMode)).ToList();
        }

        public async Task<IReadOnlyList<BankResponse>> GetByCountryIdAsync(
            int countryId, bool treeMode = false, CancellationToken cancellationToken = default)
        {
            var query = _repo.Query()
                .AsNoTracking()
                .Include(b => b.Country)
                .Include(b => b.Branches)
                .Where(b => b.CountryId == countryId);

            if (treeMode)
                query = query.Where(b => b.ParentBankId == null);

            var banks = await query.OrderBy(b => b.Name).ToListAsync(cancellationToken);
            return banks.Select(b => Map(b, includeBranches: treeMode)).ToList();
        }

        public async Task<List<LookupItem<int>>> GetLookupAsync(CancellationToken cancellationToken = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(b => b.IsActive)
                .OrderBy(b => b.Name)
                .Select(b => new LookupItem<int>(b.Id, b.Name))
                .ToListAsync(cancellationToken);
        }

        public async Task<List<LookupItem<int>>> GetParentBanksLookupAsync(
            int? excludeBankId = null, CancellationToken cancellationToken = default)
        {
            var banks = await _repo.Query()
                .AsNoTracking()
                .Where(b => b.IsActive)
                .Select(b => new { b.Id, b.Name, b.ParentBankId })
                .ToListAsync(cancellationToken);

            var excluded = new HashSet<int>();
            if (excludeBankId.HasValue)
            {
                excluded.Add(excludeBankId.Value);
                CollectDescendants(excludeBankId.Value, banks.Select(b => (b.Id, b.ParentBankId)).ToList(), excluded);
            }

            return banks
                .Where(b => !excluded.Contains(b.Id))
                .OrderBy(b => b.Name)
                .Select(b => new LookupItem<int>(b.Id, b.Name))
                .ToList();
        }

        private static void CollectDescendants(int parentId, List<(int Id, int? ParentBankId)> all, HashSet<int> excluded)
        {
            var children = all.Where(b => b.ParentBankId == parentId).ToList();
            foreach (var child in children)
            {
                if (excluded.Add(child.Id))
                    CollectDescendants(child.Id, all, excluded);
            }
        }

        // ── Commands ─────────────────────────────────────────────
        public async Task<BankResponse> CreateAsync(CreateBankRequest request, CancellationToken cancellationToken = default)
        {
            var bank = Bank.Create(
                request.Name, request.ShortName, request.LegalName, request.ParentBankId,
                request.SwiftBic, request.BranchCode, request.ClearingCode, request.NationalBankCode,
                request.CountryId, request.City, request.Address, request.Website, request.Note);

            await _repo.AddAsync(bank, cancellationToken);
            await _repo.SaveChangesAsync(cancellationToken);

            var created = await GetByIdAsync(bank.Id, cancellationToken);
            return created!;
        }

        public async Task<Result> UpdateAsync(UpdateBankRequest request, CancellationToken cancellationToken = default)
        {
            var bank = await _repo.Query().FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);
            if (bank is null)
                return Result.Fail("Bank not found.");

            try
            {
                bank.Update(
                    request.Name, request.ShortName, request.LegalName, request.ParentBankId,
                    request.SwiftBic, request.BranchCode, request.ClearingCode, request.NationalBankCode,
                    request.CountryId, request.City, request.Address, request.Website, request.Note);
            }
            catch (InvalidOperationException ex)
            {
                return Result.Fail(ex.Message);
            }

            _repo.Update(bank);
            await _repo.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }

        public async Task<Result> DeactivateAsync(int id, CancellationToken cancellationToken = default)
        {
            var bank = await _repo.Query().FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
            if (bank is null) return Result.Fail("Bank not found.");

            bank.Deactivate();
            _repo.Update(bank);
            await _repo.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }

        public async Task<Result> ActivateAsync(int id, CancellationToken cancellationToken = default)
        {
            var bank = await _repo.Query().FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
            if (bank is null) return Result.Fail("Bank not found.");

            bank.Activate();
            _repo.Update(bank);
            await _repo.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }

        public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var bank = await _repo.Query()
                .Include(b => b.Branches)
                .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
            if (bank is null) return Result.Fail("Bank not found.");

            if (bank.Branches.Any())
                return Result.Fail("Cannot delete a bank that has branches. Reassign or delete branches first.");

            _repo.HardDelete(bank);
            await _repo.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }

        // ── Emails ───────────────────────────────────────────────
        public async Task<Result> AddEmailAsync(int bankId, AddEmailRequest request, CancellationToken cancellationToken = default)
        {
            var bank = await _repo.Query().FirstOrDefaultAsync(b => b.Id == bankId, cancellationToken);
            if (bank is null) return Result.Fail("Bank not found.");
            
            var email = ContactEmail.Create(request.Address, request.Role, request.Label, request.IsPrimary);
            var result = bank.AddEmail(email);
            if (!result.IsSuccess) 
                return result;

            _repo.Update(bank);
            await _repo.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }

        public async Task<Result> RemoveEmailAsync(int bankId, string email, CancellationToken cancellationToken = default)
        {
            var bank = await _repo.Query().FirstOrDefaultAsync(b => b.Id == bankId, cancellationToken);
            if (bank is null) return Result.Fail("Bank not found.");

            var result = bank.RemoveEmail(email);
            if (!result.IsSuccess) return result;

            _repo.Update(bank);
            await _repo.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }

        // ── Phones ───────────────────────────────────────────────
        public async Task<Result> AddPhoneAsync(int bankId, AddPhoneRequest request, CancellationToken cancellationToken = default)
        {
            var bank = await _repo.Query().FirstOrDefaultAsync(b => b.Id == bankId, cancellationToken);
            if (bank is null) return Result.Fail("Bank not found.");

            var phone = ContactPhone.Create(request.Number, request.Type, request.Label, request.IsPrimary);
            var result = bank.AddPhone(phone);
            if (!result.IsSuccess) return result;

            _repo.Update(bank);
            await _repo.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }

        public async Task<Result> RemovePhoneAsync(int bankId, string phoneNumber, CancellationToken cancellationToken = default)
        {
            var bank = await _repo.Query().FirstOrDefaultAsync(b => b.Id == bankId, cancellationToken);
            if (bank is null) return Result.Fail("Bank not found.");

            var result = bank.RemovePhone(phoneNumber);
            if (!result.IsSuccess) return result;

            _repo.Update(bank);
            await _repo.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }

        // ── Mapping ──────────────────────────────────────────────
        private static BankResponse Map(Bank b, bool includeBranches)
        {
            var response = new BankResponse
            {
                BankId = b.Id,
                BankName = b.Name,
                ShortName = b.ShortName,
                LegalName = b.LegalName,
                ParentBankId = b.ParentBankId,
                ParentBankName = b.ParentBank?.Name,
                SwiftBic = b.SwiftBic,
                BranchCode = b.BranchCode,
                ClearingCode = b.ClearingCode,
                NationalBankCode = b.NationalBankCode,
                CountryId = b.CountryId,
                CountryName = b.Country?.CountryName,
                City = b.City,
                Address = b.Address,
                Website = b.Website,
                Notes = b.Notes,
                IsActive = b.IsActive,
                Emails = b.Emails.Select(e => new EmailsResponse
                {
                    Address = e.Address,
                    Role = e.Role,
                    Label = e.Label,
                    IsPrimary = e.IsPrimary
                }).ToList(),
                Phones = b.Phones.Select(p => new PhonesResponse
                {
                    Number = p.Number,
                    Type = p.Type,
                    Label = p.Label,
                    IsPrimary = p.IsPrimary
                }).ToList()
            };

            if (includeBranches)
                response.Branches = b.Branches.Select(br => Map(br, includeBranches: false)).ToList();

            return response;
        }
    }
}