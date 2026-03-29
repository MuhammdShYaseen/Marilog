using Marilog.Application.DTOs.Responses;
using Marilog.Application.Interfaces.Services;
using Marilog.Presentation.DTOs.PortDTOs;
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
            return Ok(result);
        }

        [HttpGet("code/{code}")]
        public async Task<ActionResult<PortResponse>> GetByCode(string code, CancellationToken ct)
        {
            var result = await _service.GetByCodeAsync(code, ct);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<PortResponse>>> GetAll(CancellationToken ct)
            => Ok(await _service.GetAllAsync(ct));

        [HttpGet("active")]
        public async Task<ActionResult<IReadOnlyList<PortResponse>>> GetActive(CancellationToken ct)
            => Ok(await _service.GetActiveAsync(ct));

        [HttpGet("country/{countryId}")]
        public async Task<ActionResult<IReadOnlyList<PortResponse>>> GetByCountry(int countryId, CancellationToken ct)
            => Ok(await _service.GetByCountryAsync(countryId, ct));

        [HttpPost]
        public async Task<ActionResult<PortResponse>> Create([FromBody] CreatePortRequest request, CancellationToken ct)
        {
            var result = await _service.CreateAsync(request.PortCode, request.PortName, request.CountryId, ct);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
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
