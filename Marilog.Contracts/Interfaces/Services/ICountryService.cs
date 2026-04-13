using Marilog.Contracts.DTOs.Requests.CountryDTOs;
using Marilog.Contracts.DTOs.Responses;

namespace Marilog.Contracts.Interfaces.Services
{
    public interface ICountryService
    {
        // ── Queries ───────────────────────────────────────────────────────────────
        Task<CountryResponse?>              GetByIdAsync(int id, CancellationToken ct = default);
        Task<CountryResponse?>              GetByCodeAsync(string code, CancellationToken ct = default);
        Task<IReadOnlyList<CountryResponse>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<CountryResponse>> GetActiveAsync(CancellationToken ct = default);
        Task<bool>                  ExistsByCodeAsync(string code, CancellationToken ct = default);

        // ── Commands ─────────────────────────────────────────────────────────────
        Task<CountryResponse> CreateAsync(string countryCode, string countryName, CancellationToken ct = default);
        Task<IReadOnlyList<CountryResponse>> CreateRangeAsync(IEnumerable<CreateCountryRequest> commands, CancellationToken ct = default);
        Task          UpdateAsync(int id, string countryCode, string countryName, CancellationToken ct = default);
        Task          ActivateAsync(int id, CancellationToken ct = default);
        Task          DeactivateAsync(int id, CancellationToken ct = default);
        Task          DeleteAsync(int id, CancellationToken ct = default);
    }
}
