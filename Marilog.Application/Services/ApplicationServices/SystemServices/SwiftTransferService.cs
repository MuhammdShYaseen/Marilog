using Marilog.Contracts.DTOs.Reports.SwiftTransferReports;
using Marilog.Contracts.DTOs.Requests.SwiftTransferDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Domain.Entities.SystemEntities;
using Marilog.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Marilog.Application.Services.ApplicationServices.SystemServices
{
    public class SwiftTransferService : ISwiftTransferService
    {
        private readonly IRepository<SwiftTransfer> _repo;

        public SwiftTransferService(IRepository<SwiftTransfer> repo) => _repo = repo;

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<SwiftTransferResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(ToResponse)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<SwiftTransferResponse?> GetByReferenceAsync(string reference,
            CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.SwiftReference == reference)
                .Select(ToResponse)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<SwiftTransferResponse?> GetWithPaymentsAsync(int id,
            CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(ToResponse)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<IReadOnlyList<SwiftTransferResponse>> GetBySenderAsync(int companyId,
            CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.SenderCompanyId == companyId && x.IsActive)
                .OrderByDescending(x => x.TransactionDate)
                .Select(ToResponse)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<SwiftTransferResponse>> GetBySenderAndReceverAsync(int senderId, int receverId, CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.SenderCompanyId == senderId && x.IsActive && x.ReceiverCompanyId == receverId)
                .OrderByDescending(x => x.TransactionDate)
                .Select(ToResponse)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<SwiftTransferResponse>> GetByReceiverAsync(int companyId,
            CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.ReceiverCompanyId == companyId && x.IsActive)
                .OrderByDescending(x => x.TransactionDate)
                .Select(ToResponse)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<SwiftTransferResponse>> GetUnallocatedAsync(
            CancellationToken ct = default)
        {
            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.IsActive &&
                            x.Amount > x.Payments.Sum(p => p.PaidAmount))
                .OrderBy(x => x.TransactionDate)
                .Select(ToResponse)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<SwiftTransferResponse>> GetByDateRangeAsync(DateOnly from,
            DateOnly to, CancellationToken ct = default)
        {
            if (from > to)
                throw new ArgumentException("From date cannot be after To date.");

            return await _repo.Query()
                .AsNoTracking()
                .Where(x => x.TransactionDate >= from &&
                            x.TransactionDate <= to &&
                            x.IsActive)
                .OrderByDescending(x => x.TransactionDate)
                .Select(ToResponse)
                .ToListAsync(ct);
        }
        //----Reports--------------------------------------------------------------
        public async Task<SwiftTransferReport> GetSwiftTransfersReportAsync(
            SwiftTransferFilterOptions options,
            CancellationToken ct = default)
        {
            var query = _repo.Query().AsNoTracking();

            // ─── فلترة ───────────────────────────────────────────────────────────
            if (options.Id.HasValue)
                query = query.Where(x => x.Id == options.Id.Value);

            if (!string.IsNullOrWhiteSpace(options.Reference))
                query = query.Where(x => x.SwiftReference == options.Reference);

            if (options.SenderCompanyId.HasValue)
                query = query.Where(x => x.SenderCompanyId == options.SenderCompanyId.Value
                                       && x.IsActive);

            if (options.ReceiverCompanyId.HasValue)
                query = query.Where(x => x.ReceiverCompanyId == options.ReceiverCompanyId.Value
                                       && x.IsActive);          // ✅ أضفنا IsActive هنا أيضاً

            if (options.FromDate.HasValue)
                query = query.Where(x => x.TransactionDate >= options.FromDate.Value);

            if (options.ToDate.HasValue)
                query = query.Where(x => x.TransactionDate <= options.ToDate.Value);

            if (options.OnlyUnallocated)
                query = query.Where(x => x.IsActive && x.UnallocatedAmount > 0); // ✅ استخدام الحقل المحسوب مباشرة

            // ─── ترتيب ───────────────────────────────────────────────────────────
            query = query.OrderByDescending(x => x.TransactionDate);

            // ─── الإحصاءات العامة من DB مباشرة (قبل جلب التفاصيل) ───────────────
            // ✅ تجنّب تحميل كل البيانات في الذاكرة لمجرد حساب المجاميع
            var summary = await query.GroupBy(_ => 1).Select(g => new
            {
                TotalAmount = g.Sum(x => x.Amount),
                TotalAllocated = g.Sum(x => x.AllocatedAmount),
                TotalUnallocated = g.Sum(x => x.UnallocatedAmount),
            }).FirstOrDefaultAsync(ct);

            // ─── جلب قائمة التحويلات ─────────────────────────────────────────────
            var transfers = await query.Select(x => new SwiftTransferResponse
            {
                Id = x.Id,
                SwiftReference = x.SwiftReference,
                SenderCompanyId = x.SenderCompanyId,
                ReceiverCompanyId = x.ReceiverCompanyId,
                TransactionDate = x.TransactionDate,
                Amount = x.Amount,
                IsActive = x.IsActive,
                AllocatedAmount = x.AllocatedAmount,
                UnallocatedAmount = x.UnallocatedAmount,
                RawMessage = x.RawMessage,
                ReceiverBankName = x.ReceiverBank!.Name,
                SenderBankName = x.SenderBank!.Name,
                ReceiverBankNav = new BankResponse
                {
                    BankId = x.ReceiverBankId,
                    BankName = x.ReceiverBank.Name,
                    CountryName = x.ReceiverBank.Country.CountryName,
                    City = x.ReceiverBank.City
                },
                SenderBankNav = new BankResponse
                {
                    BankId = x.SenderBankId,
                    BankName = x.SenderBank.Name,
                    CountryName = x.SenderBank.Country.CountryName,
                    City = x.SenderBank.City
                },
                IsFullyAllocated = x.IsFullyAllocated,
                SenderBankId = x.ReceiverBankId,
                ReceiverBankId = x.ReceiverBankId,
                PaymentReference = x.PaymentReference,
                CurrencyCode = x.Currency.CurrencyCode,
                CurrencyId = x.CurrencyId,
                SenderCompanyName = x.SenderCompany!.CompanyName,
                ReceiverCompanyName = x.ReceiverCompany!.CompanyName,

                Payments = options.IncludePayments
                    ? x.Payments.Select(p => new PaymentResponse
                    {
                        Id = p.Id,
                        PaidAmount = p.PaidAmount,
                        SwiftTransferId = p.SwiftTransferId,
                        DocumentId = p.DocumentId,
                        PaymentDate = p.PaymentDate,
                    }).ToList()
                    : null   // ✅ null أوضح من قائمة فارغة لتمييز "لم يُطلب" عن "لا يوجد"

            }).ToListAsync(ct);

            // ─── التجميع الشهري (In-Memory — البيانات محدودة بعد الفلترة) ──────────
            var monthlySummary = transfers
                .GroupBy(t => new { t.TransactionDate.Year, t.TransactionDate.Month })
                .Select(g => new MonthlyTransferSummary
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalAmount = g.Sum(t => t.Amount),
                    TotalPaid = g.Sum(t => t.AllocatedAmount),      // ✅ من الحقل مباشرة
                    TotalUnallocated = g.Sum(t => t.UnallocatedAmount),    // ✅ من الحقل مباشرة
                })
                .OrderBy(m => m.Year)
                .ThenBy(m => m.Month)
                .ToList();

            // ─── التجميع حسب المُرسِل ────────────────────────────────────────────
            var senderSummary = transfers
                .Where(t => t.SenderCompanyId.HasValue)                    // ✅ تحقق آمن من null
                .GroupBy(t => t.SenderCompanyId!.Value)
                .Select(g => new CompanyTransferSummary
                {
                    CompanyId = g.Key,
                    CompanyName = g.First().SenderCompanyName ?? string.Empty,
                    TotalAmount = g.Sum(t => t.Amount),
                    TotalPaid = g.Sum(t => t.AllocatedAmount),
                    TotalUnallocated = g.Sum(t => t.UnallocatedAmount),
                    TransfersCount = g.Count()
                })
                .OrderBy(c => c.CompanyName)
                .ToList();

            // ─── التجميع حسب المُستقبِل ──────────────────────────────────────────
            var receiverSummary = transfers
                .Where(t => t.ReceiverCompanyId.HasValue)                  // ✅ تحقق آمن من null
                .GroupBy(t => t.ReceiverCompanyId!.Value)
                .Select(g => new CompanyTransferSummary
                {
                    CompanyId = g.Key,
                    CompanyName = g.First().ReceiverCompanyName ?? string.Empty,
                    TotalAmount = g.Sum(t => t.Amount),
                    TotalPaid = g.Sum(t => t.AllocatedAmount),
                    TotalUnallocated = g.Sum(t => t.UnallocatedAmount),
                    TransfersCount = g.Count()
                })
                .OrderBy(c => c.CompanyName)
                .ToList();

            // ─── تجميع النتيجة النهائية ───────────────────────────────────────────
            return new SwiftTransferReport
            {
                Transfers = transfers,
                TotalAmount = summary?.TotalAmount ?? 0m,
                TotalPaid = summary?.TotalAllocated ?? 0m,
                TotalUnallocated = summary?.TotalUnallocated ?? 0m,
                MonthlySummary = monthlySummary,
                SenderSummary = senderSummary,
                ReceiverSummary = receiverSummary,
            };
        }
        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<SwiftTransferResponse> CreateAsync(string swiftReference,
            DateOnly transactionDate, int currencyId, decimal amount,
            int? senderCompanyId = null, int? receiverCompanyId = null,
            int? senderBankId = null, int? receiverBankId = null,
            string? paymentReference = null, string? rawMessage = null,
            CancellationToken ct = default)
        {
            await EnsureUniqueReferenceAsync(swiftReference, excludeId: null, ct);

            var transfer = SwiftTransfer.Create(swiftReference, transactionDate, currencyId,
                                                amount, senderCompanyId, receiverCompanyId,
                                                senderBankId, receiverBankId,
                                                paymentReference, rawMessage);
            await _repo.AddAsync(transfer, ct);
            await _repo.SaveChangesAsync(ct);
            return new SwiftTransferResponse
            {
                SwiftReference = transfer.SwiftReference,
                SenderBankId = transfer.SenderBankId,
                TransactionDate = transfer.TransactionDate,
                SenderCompanyId = transfer.SenderCompanyId,
                Amount = transfer.Amount,
                ReceiverCompanyId = transfer.ReceiverCompanyId,
                ReceiverBankId = transfer.ReceiverBankId,
                PaymentReference = transfer.PaymentReference,
                RawMessage = transfer.RawMessage,
            };
        }

        public async Task<IReadOnlyList<SwiftTransferResponse>> CreateRangeAsync(
        IEnumerable<CreateSwiftTransferRequest> commands,
        CancellationToken ct = default)
        {
            var commandList = commands.ToList();
            if (!commandList.Any())
                return Array.Empty<SwiftTransferResponse>();

            // --- تحقق من التكرار داخل الـ batch ---
            var referenceSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var c in commandList)
            {
                if (!referenceSet.Add(c.SwiftReference))
                    throw new InvalidOperationException(
                        $"Duplicate SwiftReference found in the request: '{c.SwiftReference}'");
            }

            // --- تحقق من التكرار في DB بـ query واحدة ---
            var references = referenceSet.ToList();
            var existingReferences = await _repo.Query()
                .Where(s => references.Contains(s.SwiftReference))
                .Select(s => s.SwiftReference)
                .ToListAsync(ct);

            if (existingReferences.Any())
                throw new InvalidOperationException(
                    $"SwiftReference(s) already exist: {string.Join(", ", existingReferences)}");

            // --- إنشاء دفعة واحدة ---
            var transfers = commandList
                .Select(c => SwiftTransfer.Create(
                    c.SwiftReference,
                    c.TransactionDate,
                    c.CurrencyId,
                    c.Amount,
                    c.SenderCompanyId,
                    c.ReceiverCompanyId,
                    c.SenderBankId,
                    c.ReceiverBankId,
                    c.PaymentReference,
                    c.RawMessage))
                .ToList();

            await _repo.AddRangeAsync(transfers, ct);
            await _repo.SaveChangesAsync(ct);

            return transfers
                .Select(transfer => new SwiftTransferResponse
                {
                    Id = transfer.Id,
                    SwiftReference = transfer.SwiftReference,
                    TransactionDate = transfer.TransactionDate,
                    CurrencyId = transfer.CurrencyId,
                    Amount = transfer.Amount,
                    SenderCompanyId = transfer.SenderCompanyId,
                    ReceiverCompanyId = transfer.ReceiverCompanyId,
                    SenderBankId = transfer.SenderBankId,
                    ReceiverBankId = transfer.ReceiverBankId,
                    PaymentReference = transfer.PaymentReference
                })
                .ToList();
        }

        public async Task UpdateAsync(int id, int currencyId, decimal amount,
            int? senderBankId, int? receiverBankId,
            string? paymentReference, string? rawMessage,
            CancellationToken ct = default)
        {
            var transfer = await GetOrThrowAsync(id, ct);
            transfer.Update(currencyId, amount, senderBankId, receiverBankId,
                            paymentReference, rawMessage);
            _repo.Update(transfer);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task ActivateAsync(int id, CancellationToken ct = default)
        {
            var transfer = await GetOrThrowAsync(id, ct);
            transfer.Activate();
            _repo.Update(transfer);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task DeactivateAsync(int id, CancellationToken ct = default)
        {
            var transfer = await GetOrThrowAsync(id, ct);
            transfer.Deactivate();
            _repo.Update(transfer);
            await _repo.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var transfer = await _repo.Query()
                .Include(x => x.Payments)
                .FirstOrDefaultAsync(x => x.Id == id, ct)
                ?? throw new KeyNotFoundException($"SwiftTransfer {id} not found.");

            if (transfer.Payments.Any())
                throw new InvalidOperationException(
                    "Cannot delete a transfer that has allocated payments. Deactivate it instead.");

            _repo.HardDelete(transfer);
            await _repo.SaveChangesAsync(ct);
        }

        // ── Private ───────────────────────────────────────────────────────────────

        private async Task<SwiftTransfer> GetOrThrowAsync(int id, CancellationToken ct)
            => await _repo.GetByIdAsync(id, ct)
               ?? throw new KeyNotFoundException($"SwiftTransfer {id} not found.");

        private async Task EnsureUniqueReferenceAsync(string reference,
            int? excludeId, CancellationToken ct)
        {
            var conflict = await _repo.Query()
                .AnyAsync(x => x.SwiftReference == reference &&
                               (excludeId == null || x.Id != excludeId), ct);
            if (conflict)
                throw new InvalidOperationException(
                    $"Swift reference '{reference}' already exists.");
        }

        private static readonly Expression<Func<SwiftTransfer, SwiftTransferResponse>> ToResponse =
            x => new SwiftTransferResponse
        {
            Id = x.Id,
            SwiftReference = x.SwiftReference,
            TransactionDate = x.TransactionDate,

            CurrencyId = x.CurrencyId,
            CurrencyCode = x.Currency.CurrencyCode,

            Amount = x.Amount,
            ReceiverBankName = x.ReceiverBank!.Name,
            SenderBankName = x.SenderBank!.Name,
            AllocatedAmount = x.Payments.Sum(p => p.PaidAmount),
            UnallocatedAmount = x.Amount - x.Payments.Sum(p => p.PaidAmount),
            IsFullyAllocated = x.Amount == x.Payments.Sum(p => p.PaidAmount),

            SenderCompanyId = x.SenderCompanyId,
            SenderCompanyName = x.SenderCompany != null
                ? x.SenderCompany.CompanyName
                : null,

            ReceiverCompanyId = x.ReceiverCompanyId,
            ReceiverCompanyName = x.ReceiverCompany != null
                ? x.ReceiverCompany.CompanyName
                : null,
            RawMessage = x.RawMessage,
            SenderBankId = x.SenderBankId,
            SenderBankNav = new BankResponse 
            { 
                BankName = x.SenderBank !=null ? x.SenderBank.Name : null ,
                LegalName = x.SenderBank !=null ? x.SenderBank.LegalName : null,
                ShortName = x.SenderBank != null ? x.SenderBank.ShortName : null,
                Country = x.SenderBank != null ? x.SenderBank.Country.CountryName : null,
                SwiftBic = x.SenderBank != null ? x.SenderBank.SwiftBic : null,
                CountryId = x.SenderBank != null ?  x.SenderBank.CountryId : null,
                CountryName = x.SenderBank != null ? x.SenderBank.Country.CountryName : null,
                Phones = x.SenderBank != null ? x.SenderBank.Phones.Select(ph => new PhonesResponse
                {
                    Label = ph.Label,
                    IsPrimary = ph.IsPrimary,
                    Number = ph.Number,
                    Type = ph.Type
                }).ToList() : null,
                Emails =  x.SenderBank != null ? x.SenderBank.Emails.Select(e => new EmailsResponse
                {
                    Address = e.Address,
                    IsPrimary = e.IsPrimary,
                    Label = e.Label,
                    Role = e.Role
                }).ToList() : null,
            },

            ReceiverBankNav = new BankResponse
            {
                BankName = x.ReceiverBank != null ? x.ReceiverBank.Name : null,
                LegalName = x.ReceiverBank != null ? x.ReceiverBank.LegalName : null,
                ShortName = x.ReceiverBank != null ? x.ReceiverBank.ShortName : null,
                Country = x.ReceiverBank != null ? x.ReceiverBank.Country.CountryName : null,
                SwiftBic = x.ReceiverBank != null ? x.ReceiverBank.SwiftBic : null,
                CountryId = x.ReceiverBank != null ? x.ReceiverBank.CountryId : null,
                CountryName = x.ReceiverBank != null ? x.ReceiverBank.Country.CountryName : null,
                Phones = x.ReceiverBank != null ? x.ReceiverBank.Phones.Select(ph => new PhonesResponse
                {
                    Label = ph.Label,
                    IsPrimary = ph.IsPrimary,
                    Number = ph.Number,
                    Type = ph.Type
                }).ToList() : null,
                Emails = x.ReceiverBank != null ? x.ReceiverBank.Emails.Select(e => new EmailsResponse
                {
                    Address = e.Address,
                    IsPrimary = e.IsPrimary,
                    Label = e.Label,
                    Role = e.Role
                }).ToList() : null,
            },

            ReceiverBankId = x.ReceiverBankId,
            PaymentReference = x.PaymentReference,
            IsActive = x.IsActive,
            Payments = x.Payments.Select(p => new PaymentResponse
            {
                PaidAmount = p.PaidAmount,
                DocumentId = p.DocumentId,
                IsActive = p.IsActive,
                PaymentDate = p.PaymentDate,
                PaymentMethod = p.PaymentMethod,
                Id = p.Id,
            }).ToList(),
        };
    }
}
