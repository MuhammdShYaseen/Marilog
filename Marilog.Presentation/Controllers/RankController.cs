using Marilog.Application.DTOs.Commands.Rank;
using Marilog.Application.DTOs.Responses;
using Marilog.Application.Interfaces.Services;
using Marilog.Domain.Entities.SystemEntities;
using Marilog.Presentation.Common;
using Marilog.Presentation.DTOs.RankDTOs;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RankController : ControllerBase
    {
        private readonly IRankService _service;

        public RankController(IRankService service)
        {
            _service = service;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RankResponse>> GetById(int id, CancellationToken ct)
        {
            var result = await _service.GetByIdAsync(id, ct);
            if (result == null) return NotFound();
            return Ok(ApiResponse<RankResponse>.Ok(result));
        }

        [HttpGet("code/{code}")]
        public async Task<ActionResult<RankResponse>> GetByCode(string code, CancellationToken ct)
        {
            var result = await _service.GetByCodeAsync(code, ct);
            if (result == null) return NotFound();
            return Ok(ApiResponse<RankResponse>.Ok(result));
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<RankResponse>>> GetAll(CancellationToken ct)
            => Ok(ApiResponse<IReadOnlyList<RankResponse>>.Ok(await _service.GetAllAsync(ct)));

        [HttpGet("active")]
        public async Task<ActionResult<IReadOnlyList<RankResponse>>> GetActive(CancellationToken ct)
            => Ok(ApiResponse<IReadOnlyList<RankResponse>>.Ok(await _service.GetActiveAsync(ct)));

        [HttpGet("department/{department}")]
        public async Task<ActionResult<IReadOnlyList<RankResponse>>> GetByDepartment(Department department, CancellationToken ct)
            => Ok(ApiResponse<IReadOnlyList<RankResponse>>.Ok(await _service.GetByDepartmentAsync(department, ct)));

        [HttpPost]
        public async Task<ActionResult<RankResponse>> Create([FromBody] CreateRankRequest request, CancellationToken ct)
        {
            var result = await _service.CreateAsync(request.RankCode, request.RankName, request.Department, ct);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, Ok(ApiResponse<RankResponse>.Ok(result)));
        }

        [HttpPost("batch")]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<RankResponse>>>> CreateRange(
        [FromBody] IEnumerable<CreateRankRequest> requests,
        CancellationToken ct)
        {
            var commands = requests.Select(r => new CreateRankCommand(
                r.RankCode,
                r.RankName,
                r.Department
            ));

            var result = await _service.CreateRangeAsync(commands, ct);
            return Ok(ApiResponse<IReadOnlyList<RankResponse>>.Ok(result));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateRankRequest request, CancellationToken ct)
        {
            await _service.UpdateAsync(id, request.RankCode, request.RankName, request.Department, ct);
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
