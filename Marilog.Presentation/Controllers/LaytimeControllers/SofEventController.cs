using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.LayTimeDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.CharterLaytimeServices;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers.LaytimeControllers
{
    [ApiController]
    [Route("api/laytime-calculations/{calculationId:int}/sof-events")]
    public sealed class SofEventController : ControllerBase
    {
        private readonly ISofEventService _service;

        public SofEventController(ISofEventService service)
            => _service = service;

        [HttpPost]
        public async Task<IActionResult> Add(
            int calculationId,
            [FromBody] AddSofEventRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.AddSofEventAsync(
                calculationId,
                request,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetAll),
                new { calculationId },
                ApiResponse<SofEventResponse>.Ok(result));
        }

        [HttpPost("batch")]
        public async Task<IActionResult> AddBatch(
            int calculationId,
            [FromBody] IEnumerable<AddSofEventRequest> requests,
            CancellationToken cancellationToken)
        {
            var result = await _service.AddSofEventsBatchAsync(
                calculationId,
                requests,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetAll),
                new { calculationId },
                ApiResponse<IReadOnlyList<SofEventResponse>>.Ok(result));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            int calculationId,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetSofEventsAsync(
                calculationId,
                cancellationToken);

            return Ok(
                ApiResponse<IReadOnlyList<SofEventResponse>>.Ok(result));
        }

        [HttpPut("{sofEventId:int}/impact")]
        public async Task<IActionResult> UpdateImpact(
            int calculationId,
            int sofEventId,
            [FromBody] UpdateSofEventImpactRequest request,
            CancellationToken cancellationToken)
        {
            await _service.UpdateSofEventImpactAsync(
                sofEventId,
                request,
                cancellationToken);

            return NoContent();
        }

        [HttpDelete("{sofEventId:int}")]
        public async Task<IActionResult> Remove(
            int calculationId,
            int sofEventId,
            CancellationToken cancellationToken)
        {
            await _service.RemoveSofEventAsync(
                calculationId,
                sofEventId,
                cancellationToken);

            return NoContent();
        }
    }
}
