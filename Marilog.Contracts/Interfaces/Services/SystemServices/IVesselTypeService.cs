using Marilog.Contracts.DTOs.Requests.VesselTypeDTOs;
using Marilog.Contracts.DTOs.Responses;

namespace Marilog.Contracts.Interfaces.Services.SystemServices
{
    public interface IVesselTypeService
    {
        Task<IReadOnlyList<VesselTypeResponse>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<VesselTypeResponse>> GetActiveAsync(CancellationToken ct = default);
        Task<VesselTypeResponse?> GetByIdAsync(int id, CancellationToken ct = default);

        Task<VesselTypeResponse> CreateAsync(CreateVesselTypeRequest request, CancellationToken ct = default);
        Task UpdateAsync(int id, UpdateVesselTypeRequest request, CancellationToken ct = default);
        Task ActivateAsync(int id, CancellationToken ct = default);
        Task DeactivateAsync(int id, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);
    }
}