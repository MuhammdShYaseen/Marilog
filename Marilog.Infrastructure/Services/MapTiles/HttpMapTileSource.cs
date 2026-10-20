using Marilog.Application.Interfaces.MapTiles;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Net;

namespace Marilog.Infrastructure.Services.MapTiles
{
    internal sealed class HttpMapTileSource : IMapTileSource
    {
        public const string HttpClientName = "MapTiles";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<HttpMapTileSource> _logger;

        public HttpMapTileSource(IHttpClientFactory httpClientFactory, ILogger<HttpMapTileSource> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<UpstreamTileResult> FetchAsync(string urlTemplate, int z, int x, int y, CancellationToken cancellationToken)
        {
            var url = urlTemplate
                .Replace("{z}", z.ToString(CultureInfo.InvariantCulture))
                .Replace("{x}", x.ToString(CultureInfo.InvariantCulture))
                .Replace("{y}", y.ToString(CultureInfo.InvariantCulture));

            try
            {
                var client = _httpClientFactory.CreateClient(HttpClientName);
                using var response = await client.GetAsync(url, cancellationToken);

                if (response.StatusCode == HttpStatusCode.NotFound)
                    return UpstreamTileResult.NotFound;

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Map tile upstream returned {StatusCode} for {Url}", (int)response.StatusCode, url);
                    return UpstreamTileResult.Failed;
                }

                var content = await response.Content.ReadAsByteArrayAsync(cancellationToken);

                if (TileImage.DetectContentType(content) is null)
                {
                    _logger.LogWarning("Map tile upstream returned a non-image body for {Url}", url);
                    return UpstreamTileResult.Failed;
                }

                return UpstreamTileResult.Ok(content);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                _logger.LogWarning(ex, "Map tile upstream request failed for {Url}", url);
                return UpstreamTileResult.Failed;
            }
        }
    }
}
