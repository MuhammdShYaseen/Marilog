using Marilog.Domain.Entities;

namespace Marilog.Domain.Interfaces.Services
{
    public interface ISwiftTransferService
    {
        // ── Queries ───────────────────────────────────────────────────────────────
        Task<SwiftTransfer?>              GetByIdAsync(int id, CancellationToken ct = default);
        Task<SwiftTransfer?>              GetByReferenceAsync(string reference, CancellationToken ct = default);
        Task<SwiftTransfer?>              GetWithPaymentsAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<SwiftTransfer>> GetBySenderAsync(int companyId, CancellationToken ct = default);
        Task<IReadOnlyList<SwiftTransfer>> GetByReceiverAsync(int companyId, CancellationToken ct = default);
        Task<IReadOnlyList<SwiftTransfer>> GetUnallocatedAsync(CancellationToken ct = default);
        Task<IReadOnlyList<SwiftTransfer>> GetByDateRangeAsync(DateOnly from, DateOnly to, CancellationToken ct = default);

        // ── Commands ─────────────────────────────────────────────────────────────
        Task<SwiftTransfer> CreateAsync(string swiftReference, DateOnly transactionDate,
                                        int currencyId, decimal amount,
                                        int? senderCompanyId = null, int? receiverCompanyId = null,
                                        string? senderBank = null, string? receiverBank = null,
                                        string? paymentReference = null, string? rawMessage = null,
                                        CancellationToken ct = default);
        Task                UpdateAsync(int id, int currencyId, decimal amount,
                                        string? senderBank, string? receiverBank,
                                        string? paymentReference, string? rawMessage,
                                        CancellationToken ct = default);
        Task                ActivateAsync(int id, CancellationToken ct = default);
        Task                DeactivateAsync(int id, CancellationToken ct = default);
        Task                DeleteAsync(int id, CancellationToken ct = default);
    }
}
