using Microsoft.EntityFrameworkCore;
using Marilog.Domain.Entities;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Application.Interfaces.Services;

namespace Marilog.Application.Services
{
    public class SwiftTransferService : ISwiftTransferService
    {
        private readonly IRepository<SwiftTransfer> _repo;

        public SwiftTransferService(IRepository<SwiftTransfer> repo) => _repo = repo;

        // ── Queries ───────────────────────────────────────────────────────────────

        public async Task<SwiftTransfer?> GetByIdAsync(int id, CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .Include(x => x.Currency)
                          .Include(x => x.SenderCompany)
                          .Include(x => x.ReceiverCompany)
                          .FirstOrDefaultAsync(x => x.Id == id, ct);

        public async Task<SwiftTransfer?> GetByReferenceAsync(string reference,
            CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .Include(x => x.Currency)
                          .Include(x => x.SenderCompany)
                          .Include(x => x.ReceiverCompany)
                          .FirstOrDefaultAsync(x => x.SwiftReference == reference, ct);

        public async Task<SwiftTransfer?> GetWithPaymentsAsync(int id,
            CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .Include(x => x.Payments)
                          .Include(x => x.Currency)
                          .FirstOrDefaultAsync(x => x.Id == id, ct);

        public async Task<IReadOnlyList<SwiftTransfer>> GetBySenderAsync(int companyId,
            CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .Where(x => x.SenderCompanyId == companyId && x.IsActive)
                          .Include(x => x.Currency)
                          .Include(x => x.ReceiverCompany)
                          .OrderByDescending(x => x.TransactionDate)
                          .ToListAsync(ct);

        public async Task<IReadOnlyList<SwiftTransfer>> GetByReceiverAsync(int companyId,
            CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .Where(x => x.ReceiverCompanyId == companyId && x.IsActive)
                          .Include(x => x.Currency)
                          .Include(x => x.SenderCompany)
                          .OrderByDescending(x => x.TransactionDate)
                          .ToListAsync(ct);

        public async Task<IReadOnlyList<SwiftTransfer>> GetUnallocatedAsync(
            CancellationToken ct = default)
            => await _repo.Query().AsNoTracking()
                          .Where(x => x.IsActive &&
                                      x.Amount > x.Payments
                                          .Where(p => p.SwiftTransferId == x.Id)
                                          .Sum(p => p.PaidAmount))
                          .Include(x => x.Currency)
                          .Include(x => x.SenderCompany)
                          .Include(x => x.ReceiverCompany)
                          .OrderBy(x => x.TransactionDate)
                          .ToListAsync(ct);

        public async Task<IReadOnlyList<SwiftTransfer>> GetByDateRangeAsync(DateOnly from,
            DateOnly to, CancellationToken ct = default)
        {
            if (from > to)
                throw new ArgumentException("From date cannot be after To date.");

            return await _repo.Query().AsNoTracking()
                              .Where(x => x.TransactionDate >= from &&
                                          x.TransactionDate <= to   &&
                                          x.IsActive)
                              .Include(x => x.Currency)
                              .Include(x => x.SenderCompany)
                              .Include(x => x.ReceiverCompany)
                              .OrderByDescending(x => x.TransactionDate)
                              .ToListAsync(ct);
        }

        // ── Commands ─────────────────────────────────────────────────────────────

        public async Task<SwiftTransfer> CreateAsync(string swiftReference,
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
            return transfer;
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
    }
}
