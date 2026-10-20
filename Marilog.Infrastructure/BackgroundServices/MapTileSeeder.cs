using Marilog.Application.Interfaces.MapTiles;
using Marilog.Application.Options;
using Marilog.Contracts.Interfaces.MapService;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Marilog.Infrastructure.BackgroundServices
{
    internal sealed class MapTileSeeder : BackgroundService
    {
        private readonly IMapTileService _tileService;
        private readonly IMapTileStore _store;
        private readonly MapTileOptions _options;
        private readonly ILogger<MapTileSeeder> _logger;

        public MapTileSeeder(
            IMapTileService tileService,
            IMapTileStore store,
            IOptions<MapTileOptions> options,
            ILogger<MapTileSeeder> logger)
        {
            _tileService = tileService;
            _store = store;
            _options = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var maxZoom = Math.Min(_options.SeedMaxZoom, _options.MaxZoom);
            if (maxZoom < 0 || _options.Layers.Count == 0)
                return;

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            var fetched = 0;

            foreach (var layer in _options.Layers.Keys)
            {
                for (var z = 0; z <= maxZoom; z++)
                {
                    var n = 1 << z;
                    for (var x = 0; x < n; x++)
                    {
                        for (var y = 0; y < n; y++)
                        {
                            if (_store.Exists(layer, z, x, y))
                                continue;

                            while (!_tileService.IsUpstreamAvailable)
                                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

                            await _tileService.GetTileAsync(layer, z, x, y, stoppingToken);
                            fetched++;

                            await Task.Delay(_options.SeedDelayMs, stoppingToken);
                        }
                    }
                }
            }

            _logger.LogInformation("Map tile seeding finished: {Count} tiles fetched up to zoom {MaxZoom}.", fetched, maxZoom);
        }
    }
}
