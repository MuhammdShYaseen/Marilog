using Marilog.Domain.Entities;

namespace Marilog.Domain.Interfaces.Services
{
    public interface ICompanyService
    {
        // ── Queries ───────────────────────────────────────────────────────────────
        Task<Company?>              GetByIdAsync(int id, CancellationToken ct = default);
        Task<Company?>              GetWithVesselsAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<Company>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<Company>> GetActiveAsync(CancellationToken ct = default);
        Task<IReadOnlyList<Company>> SearchByNameAsync(string name, CancellationToken ct = default);

        // ── Commands ─────────────────────────────────────────────────────────────
        Task<Company> CreateAsync(string companyName, int? countryId = null,
                                  string? contactName = null, string? email = null,
                                  string? phone = null, string? address = null,
                                  CancellationToken ct = default);
        Task          UpdateAsync(int id, string companyName, int? countryId = null,
                                  string? contactName = null, string? email = null,
                                  string? phone = null, string? address = null,
                                  CancellationToken ct = default);
        Task          ActivateAsync(int id, CancellationToken ct = default);
        Task          DeactivateAsync(int id, CancellationToken ct = default);
        Task          DeleteAsync(int id, CancellationToken ct = default);
    }
}
