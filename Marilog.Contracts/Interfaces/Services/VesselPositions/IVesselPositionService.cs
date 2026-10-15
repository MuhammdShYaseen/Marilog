

using Marilog.Contracts.DTOs.Responses;

namespace Marilog.Contracts.Interfaces.Services.VesselPositions
{
    public interface IVesselPositionService
    {
        Task<VesselPositionResponse?> GetByImoAsync(int imo, CancellationToken ct = default);
    }
}
