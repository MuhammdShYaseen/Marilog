using Marilog.Application.DTOs;
using Marilog.Application.Interfaces.Services;
using Marilog.Domain.Entities;
using Marilog.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Marilog.Application.Services
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
