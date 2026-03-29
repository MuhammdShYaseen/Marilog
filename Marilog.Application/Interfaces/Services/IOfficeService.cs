using Marilog.Application.DTOs.Responses;
using Marilog.Domain.Entities;

namespace Marilog.Application.Interfaces.Services
{
    public interface IOfficeService
    {
        // ── Queries ───────────────────────────────────────────────────────────────
        Task<OfficeResponse?>              GetByIdAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<OfficeResponse>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<OfficeResponse>> GetActiveAsync(CancellationToken ct = default);
        Task<IReadOnlyList<OfficeResponse>> GetByCountryAsync(int countryId, CancellationToken ct = default);

        // ── Commands ─────────────────────────────────────────────────────────────
        Task<OfficeResponse> CreateAsync(string officeName, string city, int countryId,
                                 string? address = null, string? phone = null,
                                 string? contactName = null, CancellationToken ct = default);
        Task         UpdateAsync(int id, string officeName, string city, int countryId,
                                 string? address = null, string? phone = null,
                                 string? contactName = null, CancellationToken ct = default);
        Task         ActivateAsync(int id, CancellationToken ct = default);
        Task         DeactivateAsync(int id, CancellationToken ct = default);
        Task         DeleteAsync(int id, CancellationToken ct = default);
    }
}
