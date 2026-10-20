

using Marilog.Contracts.DTOs.Responses;

namespace Marilog.Contracts.Interfaces.MapService
{
    public interface IMapTileService
    {
        bool IsUpstreamAvailable { get; }
        Task<MapTileResponse?> GetTileAsync(string layer, int z, int x, int y, CancellationToken cancellationToken = default);
    }
}
