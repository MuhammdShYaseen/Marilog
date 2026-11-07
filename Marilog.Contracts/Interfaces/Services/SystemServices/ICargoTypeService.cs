using Marilog.Contracts.DTOs.Requests.CargoTypeDTOs;
using Marilog.Contracts.DTOs.Responses;

namespace Marilog.Contracts.Interfaces.Services.SystemServices
{
    public interface ICargoTypeService
    {
        Task<IReadOnlyList<CargoTypeResponse>> GetAllAsync(CancellationToken ct = default);
        Task<IReadOnlyList<CargoTypeResponse>> GetActiveAsync(CancellationToken ct = default);
        Task<CargoTypeResponse?> GetByIdAsync(int id, CancellationToken ct = default);

        Task<CargoTypeResponse> CreateAsync(CreateCargoTypeRequest request, CancellationToken ct = default);

        Task<IReadOnlyList<CargoTypeResponse>> CreateRangeAsync(IEnumerable<CreateCargoTypeRequest> requests, CancellationToken ct = default);
        Task UpdateAsync(int id, UpdateCargoTypeRequest request, CancellationToken ct = default);
        Task ActivateAsync(int id, CancellationToken ct = default);
        Task DeactivateAsync(int id, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);
    }
}