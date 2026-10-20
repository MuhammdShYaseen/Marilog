
using Marilog.Contracts.Interfaces.MapService;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers.WMap
{
    [ApiController]
    [Route("api/map-tiles")]
    public sealed class MapTilesController : ControllerBase
    {
        private readonly IMapTileService _mapTileService;

        public MapTilesController(IMapTileService mapTileService)
        {
            _mapTileService = mapTileService;
        }

        [HttpGet("{layer}/{z:int}/{y:int}/{x:int}")]
        public async Task<IActionResult> GetTile(string layer, int z, int y, int x, CancellationToken cancellationToken)
        {
            var tile = await _mapTileService.GetTileAsync(layer, z, x, y, cancellationToken);

            if (tile is null)
            {
                Response.Headers.CacheControl = "no-store";
                return NotFound();
            }

            Response.Headers.CacheControl = "public, max-age=2592000, immutable";
            return File(tile.Content, tile.ContentType);
        }
    }
}
