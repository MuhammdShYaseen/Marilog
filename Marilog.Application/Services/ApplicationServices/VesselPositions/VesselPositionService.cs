

using Marilog.Application.Interfaces.VesselPosition;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.VesselPositions;
using Microsoft.Extensions.Caching.Memory;

namespace Marilog.Application.Services.ApplicationServices.VesselPositions
{
    public sealed class VesselPositionService(IVesselPositionProvider provider, IMemoryCache cache) : IVesselPositionService
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);
        private static readonly TimeSpan NotFoundCacheDuration = TimeSpan.FromMinutes(5);

        public async Task<VesselPositionResponse?> GetByImoAsync(int imo, CancellationToken ct = default)
        {
            var key = $"vessel-position:{imo}";
            if (cache.TryGetValue(key, out VesselPositionResponse? cached))
                return cached; // may be a cached "not found"

            var response = await provider.GetByImoAsync(imo, ct);
            cache.Set(key, response, response is null ? NotFoundCacheDuration : CacheDuration);
            return response;
        }
    }
}
