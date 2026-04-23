using Marilog.Contracts.DTOs.Requests.DocumentTypeDTOs;
using Marilog.Contracts.DTOs.Responses;

namespace Marilog.Contracts.Interfaces.Services.SystemServices
{
    public interface IDocumentTypeService
    {
        // ── Queries ───────────────────────────────────────────────────────────────
        Task<DocumentTypeResponse?>              GetByIdAsync(int id, CancellationToken ct = default);
        Task<DocumentTypeResponse?>              GetByCodeAsync(string code, CancellationToken ct = default);
        Task<IReadOnlyList<DocumentTypeResponse>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<DocumentTypeResponse>> GetActiveAsync(CancellationToken ct = default);

        // ── Commands ─────────────────────────────────────────────────────────────
        Task<DocumentTypeResponse> CreateAsync(string code, string name, int sortOrder = 0, CancellationToken ct = default);
        Task<IReadOnlyList<DocumentTypeResponse>> CreateRangeAsync(IEnumerable<CreateDocumentTypeRequest> commands, CancellationToken ct = default);
        Task               UpdateAsync(int id, string name, int sortOrder, CancellationToken ct = default);
        Task               ActivateAsync(int id, CancellationToken ct = default);
        Task               DeactivateAsync(int id, CancellationToken ct = default);
        Task               DeleteAsync(int id, CancellationToken ct = default);
    }
}
