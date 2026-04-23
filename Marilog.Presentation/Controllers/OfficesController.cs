

using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.OfficeDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers
{
    [ApiController]
    [Route("api/offices")]
    public class OfficesController : ControllerBase
    {
        private readonly IOfficeService _service;

        public OfficesController(IOfficeService service)
        {
            _service = service;
        }

        // ─────────────────────────────────────────────
        // Queries
        // ─────────────────────────────────────────────

        [HttpGet("{id:int}")]
        public async Task<ActionResult<OfficeResponse>> GetById(int id, CancellationToken ct)
        {
            var office = await _service.GetByIdAsync(id, ct);
            return office is null ? NotFound() : Ok(ApiResponse<OfficeResponse>.Ok(office));
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<OfficeResponse>>> GetAll(CancellationToken ct)
        {
            var offices = await _service.GetAllAsync(ct);
            return offices is null ? NotFound() : Ok(ApiResponse<IReadOnlyList<OfficeResponse>>.Ok(offices));
        }

        [HttpGet("active")]
        public async Task<ActionResult<IReadOnlyList<OfficeResponse>>> GetActive(CancellationToken ct)
        {
           var offices = await _service.GetActiveAsync(ct);
            return offices is null ? NotFound() : Ok(ApiResponse<IReadOnlyList<OfficeResponse>>.Ok(offices));
        }

        [HttpGet("by-country/{countryId:int}")]
        public async Task<ActionResult<IReadOnlyList<OfficeResponse>>> GetByCountry(int countryId, CancellationToken ct)
        {
            var offices = await _service.GetByCountryAsync(countryId, ct);
            return offices is null ? NotFound() : Ok(ApiResponse<IReadOnlyList<OfficeResponse>>.Ok(offices));
        }

        // ─────────────────────────────────────────────
        // Commands
        // ─────────────────────────────────────────────

        [HttpPost]
        public async Task<ActionResult<OfficeResponse>> Create(
            [FromBody] CreateOfficeRequest request,
            CancellationToken ct)
        {
            var office = await _service.CreateAsync(
                request.OfficeName,
                request.City,
                request.CountryId,
                request.Address,
                request.Phone,
                request.ContactName,
                ct);

            return CreatedAtAction(nameof(GetById), new { id = office.Id }, ApiResponse<OfficeResponse>.Ok(office));
        }

        [HttpPost("batch")]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<OfficeResponse>>>> CreateRange(
        [FromBody] IEnumerable<CreateOfficeRequest> requests,
        CancellationToken ct)
        {
            var commands = requests.Select(r => new CreateOfficeRequest{
                OfficeName = r.OfficeName,
                City = r.City,
                CountryId = r.CountryId,
                Address = r.Address,
                Phone = r.Phone,
                ContactName = r.ContactName
            });

            var result = await _service.CreateRangeAsync(commands, ct);
            return Ok(ApiResponse<IReadOnlyList<OfficeResponse>>.Ok(result));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateOfficeRequest request,
            CancellationToken ct)
        {
            await _service.UpdateAsync(
                id,
                request.OfficeName,
                request.City,
                request.CountryId,
                request.Address,
                request.Phone,
                request.ContactName,
                ct);

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
