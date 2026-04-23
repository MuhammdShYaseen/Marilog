using Marilog.Contracts.DTOs.Requests.RankDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Kernel.Enums;


namespace Marilog.Contracts.Interfaces.Services.SystemServices
{
    public interface IRankService
    {
        // ── Queries ───────────────────────────────────────────────────────────────
        Task<RankResponse?>              GetByIdAsync(int id, CancellationToken ct = default);
        Task<RankResponse?>              GetByCodeAsync(string code, CancellationToken ct = default);
        Task<IReadOnlyList<RankResponse>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<RankResponse>> GetActiveAsync(CancellationToken ct = default);
        Task<IReadOnlyList<RankResponse>> GetByDepartmentAsync(Department department, CancellationToken ct = default);

        // ── Commands ─────────────────────────────────────────────────────────────
        Task<RankResponse> CreateAsync(string rankCode, string rankName, Department department, CancellationToken ct = default);
        Task<IReadOnlyList<RankResponse>> CreateRangeAsync(IEnumerable<CreateRankRequest> commands, CancellationToken ct = default);
        Task       UpdateAsync(int id, string rankCode, string rankName, Department department, CancellationToken ct = default);
        Task       ActivateAsync(int id, CancellationToken ct = default);
        Task       DeactivateAsync(int id, CancellationToken ct = default);
        Task       DeleteAsync(int id, CancellationToken ct = default);
    }
}
