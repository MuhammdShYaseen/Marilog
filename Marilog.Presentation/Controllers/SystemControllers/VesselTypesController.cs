using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.VesselTypeDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers.SystemControllers
{
    [ApiController]
    [Route("api/vessel-types")]
    public class VesselTypesController : ControllerBase
    {
        private readonly IVesselTypeService _service;

        public VesselTypesController(IVesselTypeService service) => _service = service;

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<VesselTypeResponse>>>> GetAll(CancellationToken ct)
            => Ok(ApiResponse<IReadOnlyList<VesselTypeResponse>>.Ok(await _service.GetAllAsync(ct)));

        [HttpGet("active")]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<VesselTypeResponse>>>> GetActive(CancellationToken ct)
            => Ok(ApiResponse<IReadOnlyList<VesselTypeResponse>>.Ok(await _service.GetActiveAsync(ct)));

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<VesselTypeResponse>>> GetById(int id, CancellationToken ct)
        {
            var vesselType = await _service.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException($"Vessel type {id} not found.");
            return Ok(ApiResponse<VesselTypeResponse>.Ok(vesselType));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<VesselTypeResponse>>> Create([FromBody] CreateVesselTypeRequest request, CancellationToken ct)
            => Ok(ApiResponse<VesselTypeResponse>.Ok(await _service.CreateAsync(request, ct)));

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateVesselTypeRequest request, CancellationToken ct)
        {
            await _service.UpdateAsync(id, request, ct);
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