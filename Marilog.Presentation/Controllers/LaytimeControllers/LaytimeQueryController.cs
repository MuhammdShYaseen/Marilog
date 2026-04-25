using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.CharterLaytimeServices;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers.LaytimeControllers
{
    [ApiController]
    [Route("api/laytime-calculations/{calculationId:int}/segments")]
    public sealed class LaytimeQueryController : ControllerBase
    {
        private readonly ILaytimeQueryService _service;

        public LaytimeQueryController(ILaytimeQueryService service)
            => _service = service;

        [HttpGet]
        public async Task<IActionResult> GetSegments(
            int calculationId,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetSegmentsAsync(
                calculationId,
                cancellationToken);

            return Ok(
                ApiResponse<IReadOnlyList<LaytimeSegmentResponse>>.Ok(result));
        }
    }
}
