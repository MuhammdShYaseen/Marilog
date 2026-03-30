using Marilog.Application.DTOs.Responses;
using Marilog.Application.Interfaces.Services;
using Marilog.Presentation.Common;
using Marilog.Presentation.DTOs.VesselDTOs;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VesselsController : ControllerBase
    {
        private readonly IVesselService _service;

        public VesselsController(IVesselService service)
        {
            _service = service;
        }

        // ── Queries ──────────────────────────────────────────────

        [HttpGet("{id:int}")]
        public async Task<ActionResult<VesselResponse>> GetById(int id, CancellationToken ct)
        {
            var result = await _service.GetByIdAsync(id, ct);
            return result is null ? NotFound() : Ok(ApiResponse<VesselResponse>.Ok(result));
        }

        [HttpGet("by-imo/{imoNumber}")]
        public async Task<ActionResult<VesselResponse>> GetByImo(string imoNumber, CancellationToken ct)
        {
            var result = await _service.GetByImoAsync(imoNumber, ct);
            return result is null ? NotFound() : Ok(ApiResponse<VesselResponse>.Ok(result));
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<VesselResponse>>> GetAll(CancellationToken ct)
        {
            var result = await _service.GetAllAsync(ct);
            return Ok(ApiResponse<IReadOnlyList<VesselResponse>>.Ok(result));
        }

        [HttpGet("active")]
        public async Task<ActionResult<IReadOnlyList<VesselResponse>>> GetActive(CancellationToken ct)
        {
            var result = await _service.GetActiveAsync(ct);
            return Ok(ApiResponse< IReadOnlyList<VesselResponse>>.Ok(result));
        }

        [HttpGet("by-company/{companyId:int}")]
        public async Task<ActionResult<IReadOnlyList<VesselResponse>>> GetByCompany(int companyId, CancellationToken ct)
        {
            var result = await _service.GetByCompanyAsync(companyId, ct);
            return Ok(ApiResponse<IReadOnlyList<VesselResponse>>.Ok(result));
        }

        // ── Commands ────────────────────────────────────────────

        [HttpPost]
        public async Task<ActionResult<VesselResponse>> Create(CreateVesselRequest request, CancellationToken ct)
        {
            var result = await _service.CreateAsync(
                request.CompanyId,
                request.VesselName,
                request.ImoNumber,
                request.GrossTonnage,
                request.FlagCountryId,
                request.Notes,
                ct
            );

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, Ok(ApiResponse<VesselResponse>.Ok(result)));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, UpdateVesselRequest request, CancellationToken ct)
        {
            await _service.UpdateAsync(
                id,
                request.VesselName,
                request.ImoNumber,
                request.GrossTonnage,
                request.FlagCountryId,
                request.Notes,
                ct
            );

            return NoContent();
        }

        [HttpPost("{id:int}/activate")]
        public async Task<IActionResult> Activate(int id, CancellationToken ct)
        {
            await _service.ActivateAsync(id, ct);
            return NoContent();
        }

        [HttpPost("{id:int}/deactivate")]
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
