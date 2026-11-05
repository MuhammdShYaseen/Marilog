using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Reports.Contract;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Kernel.Primitives;

namespace Marilog.Contracts.Interfaces.Services.SystemServices
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
        Task<Result> CreateAsync(string contractNumber, int contractTypeId, DateOnly effectiveDate, DateOnly? expiryDate, string? notes, CancellationToken ct = default);
        Task<Result> UpdateAsync(int id, string contractNumber, int contractTypeId, DateOnly effectiveDate, DateOnly? expiryDate, string? notes, CancellationToken ct = default);
        Task<Result> DeleteAsync(int id, CancellationToken ct = default);
        Task<Result> ActivateAsync(int id, CancellationToken ct = default);
        Task<Result> SuspendAsync(int id, string reason, CancellationToken ct = default);
        Task<Result> TerminateAsync(int id, string reason, CancellationToken ct = default);
        Task<Result> MarkExpiredAsync(int id, CancellationToken ct = default);
        Task<Result> AddPartyAsync(int id, int companyId, int roleId, CancellationToken ct = default);
        Task<Result> RemovePartyAsync(int id, int companyId, int roleId, CancellationToken ct = default);
        Task<Result> RecordAmendmentAsync(int id, string description, DateOnly effectiveDate, CancellationToken ct = default);
        Task<Result> ChangeExpiryAsync(int id, DateOnly newExpiryDate, CancellationToken ct = default);
    }
}
