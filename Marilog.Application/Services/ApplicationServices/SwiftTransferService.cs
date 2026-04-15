using Marilog.Application.DTOs.Commands.SwiftTransfer;
using Marilog.Application.DTOs.Reports.SwiftTransferReports;
using Marilog.Application.DTOs.Responses;
using Marilog.Application.Interfaces.Services;
using Marilog.Domain.Entities.SystemEntities;
using Marilog.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Marilog.Application.Services.ApplicationServices
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
                IsFullyAllocated = x.IsFullyAllocated,
                SenderBank = x.SenderBank,
                ReceiverBank = x.ReceiverBank,
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
            string? senderBank = null, string? receiverBank = null,
            string? paymentReference = null, string? rawMessage = null,
            CancellationToken ct = default)
        {
            await EnsureUniqueReferenceAsync(swiftReference, excludeId: null, ct);

            var transfer = SwiftTransfer.Create(swiftReference, transactionDate, currencyId,
                                                amount, senderCompanyId, receiverCompanyId,
                                                senderBank, receiverBank,
                                                paymentReference, rawMessage);
            await _repo.AddAsync(transfer, ct);
            await _repo.SaveChangesAsync(ct);
            return new SwiftTransferResponse
            {
                SwiftReference = swiftReference,
                SenderBank = senderBank,
                TransactionDate = transactionDate,
                SenderCompanyId = senderCompanyId,
                Amount = amount,
                ReceiverCompanyId = receiverCompanyId,
                ReceiverBank = receiverBank,
                PaymentReference = paymentReference
            };
        }

        public async Task<IReadOnlyList<SwiftTransferResponse>> CreateRangeAsync(
        IEnumerable<CreateSwiftTransferCommand> commands,
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
                    c.SenderBank,
                    c.ReceiverBank,
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
                    SenderBank = transfer.SenderBank,
                    ReceiverBank = transfer.ReceiverBank,
                    PaymentReference = transfer.PaymentReference
                })
                .ToList();
        }

        public async Task UpdateAsync(int id, int currencyId, decimal amount,
            string? senderBank, string? receiverBank,
            string? paymentReference, string? rawMessage,
            CancellationToken ct = default)
        {
            var transfer = await GetOrThrowAsync(id, ct);
            transfer.Update(currencyId, amount, senderBank, receiverBank,
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

            SenderBank = x.SenderBank,
            ReceiverBank = x.ReceiverBank,
            PaymentReference = x.PaymentReference,
            IsActive = x.IsActive
        };
    }
}
