using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.VesselPositions;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers.VesselPosition
{
    [ApiController]
    [Route("api/vessel-positions")]
    public sealed class VesselPositionsController(IVesselPositionService service) : ControllerBase
    {
        [HttpGet("{imo:int:range(1000000,9999999)}")]
        public async Task<ActionResult<VesselPositionResponse>> GetByImo(int imo, CancellationToken ct)
            => await service.GetByImoAsync(imo, ct) is { } response
                ? Ok(response)
                : NotFound();
    }
}
