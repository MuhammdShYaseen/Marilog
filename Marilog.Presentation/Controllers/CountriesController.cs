using Marilog.Application.DTOs;
using Marilog.Application.Interfaces.Services;
using Marilog.Presentation.DTOs.CountryDTOs;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers
{
    [ApiController]
    [Route("api/countries")]
    public class CountriesController : ControllerBase
    {
        private readonly ICountryService _service;

        public CountriesController(ICountryService service)
        {
            _service = service;
        }

        // ─────────────────────────────────────────────
        // Queries
        // ─────────────────────────────────────────────

        [HttpGet("{id:int}")]
        public async Task<ActionResult<CountryResponse>> GetById(int id, CancellationToken ct)
        {
            var country = await _service.GetByIdAsync(id, ct);
            return country is null ? NotFound() : Ok(country);
        }

        [HttpGet("by-code/{code}")]
        public async Task<ActionResult<CountryResponse>> GetByCode(string code, CancellationToken ct)
        {
            var country = await _service.GetByCodeAsync(code, ct);
            return country is null ? NotFound() : Ok(country);
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<CountryResponse>>> GetAll(CancellationToken ct)
        {
            var countries = await _service.GetAllAsync(ct);
            return Ok(countries);
        }

        [HttpGet("active")]
        public async Task<ActionResult<IReadOnlyList<CountryResponse>>> GetActive(CancellationToken ct)
        {
            var countries = await _service.GetActiveAsync(ct);
            return Ok(countries);
        }

        // ─────────────────────────────────────────────
        // Commands
        // ─────────────────────────────────────────────

        [HttpPost]
        public async Task<ActionResult<CountryResponse>> Create(
            [FromBody] CreateCountryRequest request,
            CancellationToken ct)
        {
            var country = await _service.CreateAsync(request.CountryCode, request.CountryName, ct);
            return CreatedAtAction(nameof(GetById), new { id = country.Id }, country);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateCountryRequest request,
            CancellationToken ct)
        {
            await _service.UpdateAsync(id, request.CountryCode, request.CountryName, ct);
            return NoContent();
        }

        [HttpPatch("{id:int}/activate")]
        public async Task<IActionResult> Activate(int id, CancellationToken ct)
        {
            await _service.ActivateAsync(id, ct);
            return NoContent();
        }

        [HttpPatch("{id:int}/deactivate")]
        public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
        {
            await _service.DeactivateAsync(id, ct);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            await _service.DeleteAsync(id, ct);
            return NoContent();
        }
    }
}
