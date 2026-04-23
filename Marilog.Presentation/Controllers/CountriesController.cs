

using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.CountryDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
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
            return country is null ? NotFound() : Ok(ApiResponse<CountryResponse>.Ok(country));
        }

        [HttpGet("by-code/{code}")]
        public async Task<ActionResult<CountryResponse>> GetByCode(string code, CancellationToken ct)
        {
            var country = await _service.GetByCodeAsync(code, ct);
            return country is null ? NotFound() : Ok(ApiResponse<CountryResponse>.Ok(country));
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<CountryResponse>>> GetAll(CancellationToken ct)
        {
            var countries = await _service.GetAllAsync(ct);
            return Ok(ApiResponse<IReadOnlyList<CountryResponse>>.Ok(countries));
        }

        [HttpGet("active")]
        public async Task<ActionResult<IReadOnlyList<CountryResponse>>> GetActive(CancellationToken ct)
        {
            var countries = await _service.GetActiveAsync(ct);
            return Ok(ApiResponse<IReadOnlyList<CountryResponse>>.Ok(countries));
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
            return CreatedAtAction(nameof(GetById), new { id = country.Id }, ApiResponse<CountryResponse>.Ok(country));
        }

        // API/Controllers/CountriesController.cs

        [HttpPost("batch")]
        public async Task<ActionResult<IReadOnlyList<CountryResponse>>> CreateRange(
            [FromBody] IEnumerable<CreateCountryRequest> requests,
            CancellationToken ct)
        {
            var commands = requests.Select(r => new CreateCountryRequest 
            {
                CountryCode = r.CountryCode,
                CountryName = r.CountryName
            });

            var result = await _service.CreateRangeAsync(commands, ct);
            return Ok(result);
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
