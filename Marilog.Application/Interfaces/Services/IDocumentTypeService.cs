using Marilog.Domain.Entities;

namespace Marilog.Application.Interfaces.Services
{
    public interface IDocumentTypeService
    {
        // ── Queries ───────────────────────────────────────────────────────────────
        Task<DocumentType?>              GetByIdAsync(int id, CancellationToken ct = default);
        Task<DocumentType?>              GetByCodeAsync(string code, CancellationToken ct = default);
        Task<IReadOnlyList<DocumentType>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<DocumentType>> GetActiveAsync(CancellationToken ct = default);

        // ── Commands ─────────────────────────────────────────────────────────────
        Task<DocumentType> CreateAsync(string code, string name, int sortOrder = 0, CancellationToken ct = default);
        Task               UpdateAsync(int id, string name, int sortOrder, CancellationToken ct = default);
        Task               ActivateAsync(int id, CancellationToken ct = default);
        Task               DeactivateAsync(int id, CancellationToken ct = default);
        Task               DeleteAsync(int id, CancellationToken ct = default);
    }
}
