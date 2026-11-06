using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.CargoTypeDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers.SystemControllers
{
    [ApiController]
    [Route("api/cargo-types")]
    public class CargoTypesController : ControllerBase
    {
        private readonly ICargoTypeService _service;

        public CargoTypesController(ICargoTypeService service) => _service = service;

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<CargoTypeResponse>>>> GetAll(CancellationToken ct)
            => Ok(ApiResponse<IReadOnlyList<CargoTypeResponse>>.Ok(await _service.GetAllAsync(ct)));

        [HttpGet("active")]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<CargoTypeResponse>>>> GetActive(CancellationToken ct)
            => Ok(ApiResponse<IReadOnlyList<CargoTypeResponse>>.Ok(await _service.GetActiveAsync(ct)));

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<CargoTypeResponse>>> GetById(int id, CancellationToken ct)
        {
            var cargoType = await _service.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException($"Cargo type {id} not found.");
            return Ok(ApiResponse<CargoTypeResponse>.Ok(cargoType));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<CargoTypeResponse>>> Create([FromBody] CreateCargoTypeRequest request, CancellationToken ct)
            => Ok(ApiResponse<CargoTypeResponse>.Ok(await _service.CreateAsync(request, ct)));

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCargoTypeRequest request, CancellationToken ct)
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