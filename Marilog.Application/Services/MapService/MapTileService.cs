using Marilog.Application.Interfaces.MapTiles;
using Marilog.Application.Options;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.MapService;
using Marilog.Kernel.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace Marilog.Application.Services.MapService
{
    internal sealed class MapTileService : IMapTileService
    {
        private readonly IMapTileStore _store;
        private readonly IMapTileSource _source;
        private readonly MapTileOptions _options;
        private readonly ILogger<MapTileService> _logger;
        private readonly ConcurrentDictionary<string, Lazy<Task<MapTileResponse?>>> _inFlight = new();
        private long _upstreamBlockedUntilTicks;

        public MapTileService(
            IMapTileStore store,
            IMapTileSource source,
            IOptions<MapTileOptions> options,
            ILogger<MapTileService> logger)
        {
            _store = store;
            _source = source;
            _options = options.Value;
            _logger = logger;
        }

        public bool IsUpstreamAvailable =>
            DateTime.UtcNow.Ticks >= Interlocked.Read(ref _upstreamBlockedUntilTicks);

        public async Task<MapTileResponse?> GetTileAsync(string layer, int z, int x, int y, CancellationToken cancellationToken = default)
        {
            layer = layer.ToLowerInvariant();

            if (!_options.Layers.TryGetValue(layer, out var urlTemplate) || !IsValidTile(z, x, y))
                return null;

            var stored = await _store.ReadAsync(layer, z, x, y, cancellationToken);
            if (stored is not null)
                return ToResponse(stored);

            if (_store.IsMarkedMissing(layer, z, x, y) || !IsUpstreamAvailable)
                return null;

            var key = $"{layer}/{z}/{x}/{y}";
            var fetch = _inFlight.GetOrAdd(key, _ => new Lazy<Task<MapTileResponse?>>(
                () => FetchAndStoreAsync(key, layer, urlTemplate, z, x, y)));

            try
            {
                return await fetch.Value.WaitAsync(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Leaflet cancels tiles constantly while panning; the fetch keeps running and still gets stored.
                return null;
            }
        }

        private async Task<MapTileResponse?> FetchAndStoreAsync(string key, string layer, string urlTemplate, int z, int x, int y)
        {
            try
            {
                var result = await _source.FetchAsync(urlTemplate, z, x, y, CancellationToken.None);

                switch (result.Status)
                {
                    case UpstreamTileStatus.Ok:
                        await _store.WriteAsync(layer, z, x, y, result.Content!, CancellationToken.None);
                        return ToResponse(result.Content!);

                    case UpstreamTileStatus.NotFound:
                        await _store.MarkMissingAsync(layer, z, x, y, CancellationToken.None);
                        return null;

                    default:
                        OpenCircuit();
                        return null;
                }
            }
            finally
            {
                _inFlight.TryRemove(key, out _);
            }
        }

        private void OpenCircuit()
        {
            var now = DateTime.UtcNow;
            var until = now.AddMinutes(_options.UpstreamCooldownMinutes).Ticks;
            var previous = Interlocked.Exchange(ref _upstreamBlockedUntilTicks, until);

            if (previous < now.Ticks)
                _logger.LogWarning("Map tile upstream failed; pausing upstream requests for {Minutes} minutes.",
                    _options.UpstreamCooldownMinutes);
        }

        private bool IsValidTile(int z, int x, int y)
        {
            if (z < 0 || z > _options.MaxZoom)
                return false;

            var n = 1 << z;
            return x >= 0 && y >= 0 && x < n && y < n;
        }

        private static MapTileResponse ToResponse(byte[] content) =>
            new(content, TileImage.DetectContentType(content) ?? "image/png");
    }
}
