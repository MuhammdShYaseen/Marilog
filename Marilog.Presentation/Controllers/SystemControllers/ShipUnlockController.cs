using Marilog.Contracts.Common;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers.SystemControllers
{
    // No EntityType for "ShipUnlock" → PermissionConvention makes every action super admin only.
    [ApiController]
    [Route("api/ship-unlock")]
    public class ShipUnlockController : ControllerBase
    {
        private readonly IShipUnlockService _service;

        public ShipUnlockController(IShipUnlockService service) => _service = service;

        [HttpGet("key")]
        public async Task<ActionResult<ApiResponse<string>>> GetShipKey([FromQuery] string imo, CancellationToken ct)
            => Ok(ApiResponse<string>.Ok(data: await _service.GetShipKeyAsync(imo, ct)));

        [HttpGet("code")]
        public async Task<ActionResult<ApiResponse<string>>> GetUnlockCode(
            [FromQuery] string imo, [FromQuery] string request, CancellationToken ct)
            => Ok(ApiResponse<string>.Ok(data: await _service.GetUnlockCodeAsync(imo, request, ct)));
    }
}