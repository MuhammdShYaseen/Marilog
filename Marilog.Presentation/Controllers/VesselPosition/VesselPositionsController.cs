using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.VesselPositions;
using Marilog.Infrastructure.Helpers.UserIdentity;
using Marilog.Kernel.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers.VesselPosition
{
    [ApiController]
    [Route("api/vessel-positions")]
    [PermissionEntity(EntityType.Vessel)]
    public sealed class VesselPositionsController(IVesselPositionService service) : ControllerBase
    {
        [HttpGet("{imo:int:range(1000000,9999999)}")]
        public async Task<ActionResult<VesselPositionResponse>> GetByImo(int imo, CancellationToken ct)
            => await service.GetByImoAsync(imo, ct) is { } response
                ? Ok(response)
                : NotFound();
    }
}
