using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.LayTimeDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.CharterLaytimeServices;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers.LaytimeControllers
{
    [ApiController]
    [Route("api/laytime-calculations/{calculationId:int}/exceptions")]
    public sealed class LaytimeExceptionController : ControllerBase
    {
        private readonly ILaytimeExceptionService _service;

        public LaytimeExceptionController(ILaytimeExceptionService service)
            => _service = service;

        [HttpPost]
        public async Task<IActionResult> Add(
            int calculationId,
            [FromBody] AddLaytimeExceptionRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.AddExceptionAsync(
                calculationId,
                request,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetAll),
                new { calculationId },
                ApiResponse<LaytimeExceptionResponse>.Ok(result));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            int calculationId,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetExceptionsAsync(
                calculationId,
                cancellationToken);

            return Ok(
                ApiResponse<IReadOnlyList<LaytimeExceptionResponse>>.Ok(result));
        }

        [HttpPut("{exceptionId:int}")]
        public async Task<IActionResult> Update(
            int calculationId,
            int exceptionId,
            [FromBody] UpdateLaytimeExceptionRequest request,
            CancellationToken cancellationToken)
        {
            await _service.UpdateExceptionAsync(
                exceptionId,
                request,
                cancellationToken);

            return NoContent();
        }

        [HttpDelete("{exceptionId:int}")]
        public async Task<IActionResult> Remove(
            int calculationId,
            int exceptionId,
            CancellationToken cancellationToken)
        {
            await _service.RemoveExceptionAsync(
                calculationId,
                exceptionId,
                cancellationToken);

            return NoContent();
        }
    }
}
