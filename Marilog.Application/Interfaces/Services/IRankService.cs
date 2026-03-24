using Marilog.Domain.Entities;
namespace Marilog.Application.Interfaces.Services
{
    public interface IRankService
    {
        // ── Queries ───────────────────────────────────────────────────────────────
        Task<Rank?>              GetByIdAsync(int id, CancellationToken ct = default);
        Task<Rank?>              GetByCodeAsync(string code, CancellationToken ct = default);
        Task<IReadOnlyList<Rank>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<Rank>> GetActiveAsync(CancellationToken ct = default);
        Task<IReadOnlyList<Rank>> GetByDepartmentAsync(Department department, CancellationToken ct = default);

        // ── Commands ─────────────────────────────────────────────────────────────
        Task<Rank> CreateAsync(string rankCode, string rankName, Department department, CancellationToken ct = default);
        Task       UpdateAsync(int id, string rankCode, string rankName, Department department, CancellationToken ct = default);
        Task       ActivateAsync(int id, CancellationToken ct = default);
        Task       DeactivateAsync(int id, CancellationToken ct = default);
        Task       DeleteAsync(int id, CancellationToken ct = default);
    }
}
