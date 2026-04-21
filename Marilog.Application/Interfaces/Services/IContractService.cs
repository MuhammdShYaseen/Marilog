using Marilog.Application.Common;
using Marilog.Application.DTOs.Reports.Contract;
using Marilog.Application.DTOs.Responses;
using Marilog.Kernel.Primitives;

namespace Marilog.Application.Interfaces.Services
{
    public interface IContractService
    {
        // ─── Queries ───────────────────────────────────────────────────────
        Task<ContractDetailResponse?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<ContractDetailResponse?> GetByNumberAsync(string number, CancellationToken ct = default);
        Task<PagedResponse<ContractSummary>> GetPagedAsync(ContractFilter filter, CancellationToken ct = default);
        Task<List<ContractSummary>> GetExpiringAsync(int withinDays, CancellationToken ct = default);
        Task<ContractReport> GetReportAsync(CancellationToken ct = default);

        // ─── Write ────────────────────────────────────────────────────────
        Task<Result> CreateAsync(string contractNumber, string type, DateOnly effectiveDate, DateOnly? expiryDate, string? notes, CancellationToken ct = default);
        Task<Result> ActivateAsync(int id, CancellationToken ct = default);
        Task<Result> SuspendAsync(int id, string reason, CancellationToken ct = default);
        Task<Result> TerminateAsync(int id, string reason, CancellationToken ct = default);
        Task<Result> MarkExpiredAsync(int id, CancellationToken ct = default);
        Task<Result> AddPartyAsync(int id, int companyId, string role, CancellationToken ct = default);
        Task<Result> RemovePartyAsync(int id, int companyId, string role, CancellationToken ct = default);
        Task<Result> RemovePartyViaAmendmentAsync(int id, int companyId, string role, int amendmentNumber, CancellationToken ct = default);
        Task<Result> RecordAmendmentAsync(int id, string description, DateOnly effectiveDate, string changedBy, CancellationToken ct = default);
        Task<Result> ExtendExpiryAsync(int id, DateOnly newExpiryDate, int amendmentNumber, CancellationToken ct = default);
        Task<Result> AttachFileAsync(int id, string fileUrl, string fileName, CancellationToken ct = default);
    }
}
