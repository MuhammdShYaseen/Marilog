

using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.PortDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PortController : ControllerBase
    {
        private readonly IPortService _service;

        public PortController(IPortService service)
        {
            _service = service;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PortResponse>> GetById(int id, CancellationToken ct)
        {
            var result = await _service.GetByIdAsync(id, ct);
            if (result == null) return NotFound();
            return Ok(ApiResponse<PortResponse>.Ok(result));
        }

        [HttpGet("code/{code}")]
        public async Task<ActionResult<PortResponse>> GetByCode(string code, CancellationToken ct)
        {
            var result = await _service.GetByCodeAsync(code, ct);
            if (result == null) return NotFound();
            return Ok(ApiResponse<PortResponse>.Ok(result));
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<PortResponse>>> GetAll(CancellationToken ct)
            => Ok(ApiResponse<IReadOnlyList<PortResponse>>.Ok(await _service.GetAllAsync(ct)));

        [HttpGet("active")]
        public async Task<ActionResult<IReadOnlyList<PortResponse>>> GetActive(CancellationToken ct)
            => Ok(ApiResponse<IReadOnlyList<PortResponse>>.Ok(await _service.GetActiveAsync(ct)));

        [HttpGet("country/{countryId}")]
        public async Task<ActionResult<IReadOnlyList<PortResponse>>> GetByCountry(int countryId, CancellationToken ct)
            => Ok(ApiResponse<IReadOnlyList<PortResponse>>.Ok(await _service.GetByCountryAsync(countryId, ct)));

        [HttpPost]
        public async Task<ActionResult<PortResponse>> Create([FromBody] CreatePortRequest request, CancellationToken ct)
        {
            var result = await _service.CreateAsync(request.PortCode, request.PortName, request.CountryId, ct);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<PortResponse>.Ok(result));
        }

        [HttpPost("batch")]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<PortResponse>>>> CreateRange(
        [FromBody] IEnumerable<CreatePortRequest> requests,
        CancellationToken ct)
        {
            var commands = requests.Select(r => new CreatePortRequest
            {
                PortCode = r.PortCode,
                PortName = r.PortName,
                CountryId = r.CountryId
            });

            var result = await _service.CreateRangeAsync(commands, ct);
            return Ok(ApiResponse<IReadOnlyList<PortResponse>>.Ok(result));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePortRequest request, CancellationToken ct)
        {
            await _service.UpdateAsync(id, request.PortCode, request.PortName, request.CountryId, ct);
            return NoContent();
        }

        [HttpPost("{id}/activate")]
        public async Task<IActionResult> Activate(int id, CancellationToken ct)
        {
            await _service.ActivateAsync(id, ct);
            return NoContent();
        }

        [HttpPost("{id}/deactivate")]
        public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
        {
            await _service.DeactivateAsync(id, ct);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            await _service.DeleteAsync(id, ct);
            return NoContent();
        }
    }
}
