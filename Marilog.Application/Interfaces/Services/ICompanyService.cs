using Marilog.Application.DTOs.Responses;
using Marilog.Domain.Entities;

namespace Marilog.Application.Interfaces.Services
{
    public interface ICompanyService
    {
        // ── Queries ───────────────────────────────────────────────────────────────
        Task<CompanyResponse?>              GetByIdAsync(int id, CancellationToken ct = default);
        Task<CompanyResponse?>              GetWithVesselsAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<CompanyResponse>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<CompanyResponse>> GetActiveAsync(CancellationToken ct = default);
        Task<IReadOnlyList<CompanyResponse>> SearchByNameAsync(string name, CancellationToken ct = default);

        // ── Commands ─────────────────────────────────────────────────────────────
        Task<CompanyResponse> CreateAsync(string? registrationNumber, string companyName, int? countryId = null,
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
