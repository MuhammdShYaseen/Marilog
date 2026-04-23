using Marilog.Contracts.DTOs.Reports.SwiftTransferReports;
using Marilog.Contracts.DTOs.Requests.SwiftTransferDTOs;
using Marilog.Contracts.DTOs.Responses;

namespace Marilog.Contracts.Interfaces.Services.SystemServices
{
    public interface ISwiftTransferService
    {
        // ── Queries ───────────────────────────────────────────────────────────────
        Task<SwiftTransferResponse?>              GetByIdAsync(int id, CancellationToken ct = default);
        Task<SwiftTransferResponse?>              GetByReferenceAsync(string reference, CancellationToken ct = default);
        Task<SwiftTransferResponse?>              GetWithPaymentsAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<SwiftTransferResponse>> GetBySenderAsync(int companyId, CancellationToken ct = default);
        Task<IReadOnlyList<SwiftTransferResponse>> GetByReceiverAsync(int companyId, CancellationToken ct = default);
        Task<IReadOnlyList<SwiftTransferResponse>> GetUnallocatedAsync(CancellationToken ct = default);
        Task<IReadOnlyList<SwiftTransferResponse>> GetByDateRangeAsync(DateOnly from, DateOnly to, CancellationToken ct = default);
        //--Reports-----------------------------------------------------------------
        Task<SwiftTransferReport> GetSwiftTransfersReportAsync(SwiftTransferFilterOptions options, CancellationToken ct = default);
        // ── Commands ─────────────────────────────────────────────────────────────
        Task<SwiftTransferResponse> CreateAsync(string swiftReference, DateOnly transactionDate,
                                        int currencyId, decimal amount,
                                        int? senderCompanyId = null, int? receiverCompanyId = null,
                                        string? senderBank = null, string? receiverBank = null,
                                        string? paymentReference = null, string? rawMessage = null,
                                        CancellationToken ct = default);

        Task<IReadOnlyList<SwiftTransferResponse>> CreateRangeAsync(
        IEnumerable<CreateSwiftTransferRequest> commands,
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
