using Marilog.Contracts.DTOs.Responses;

namespace Marilog.Application.Interfaces.VesselPosition
{
    public interface IVesselPositionProvider
    {
        Task<VesselPositionResponse?> GetByImoAsync(int imo, CancellationToken ct = default);
    }
}
