using Marilog.Contracts.DTOs.Requests.PortDTOs;
using Marilog.Contracts.DTOs.Responses;

namespace Marilog.Contracts.Interfaces.Services
{
    public interface IPortService
    {
        // ── Queries ───────────────────────────────────────────────────────────────
        Task<PortResponse?>              GetByIdAsync(int id, CancellationToken ct = default);
        Task<PortResponse?>              GetByCodeAsync(string code, CancellationToken ct = default);
        Task<IReadOnlyList<PortResponse>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<PortResponse>> GetActiveAsync(CancellationToken ct = default);
        Task<IReadOnlyList<PortResponse>> GetByCountryAsync(int countryId, CancellationToken ct = default);
        Task<bool>               ExistsByCodeAsync(string code, CancellationToken ct = default);

        // ── Commands ─────────────────────────────────────────────────────────────
        Task<PortResponse> CreateAsync(string portCode, string portName, int? countryId = null, CancellationToken ct = default);
        Task<IReadOnlyList<PortResponse>> CreateRangeAsync(IEnumerable<CreatePortRequest> commands, CancellationToken ct = default);
        Task       UpdateAsync(int id, string portCode, string portName, int? countryId = null, CancellationToken ct = default);
        Task       ActivateAsync(int id, CancellationToken ct = default);
        Task       DeactivateAsync(int id, CancellationToken ct = default);
        Task       DeleteAsync(int id, CancellationToken ct = default);
    }
}
