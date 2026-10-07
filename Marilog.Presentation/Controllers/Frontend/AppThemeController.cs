using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Frontend.AppTheme;
using Marilog.Contracts.Interfaces.FrontendServices;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers.Frontend
{
    [ApiController]
    [Route("api/app-themes")]
    public class AppThemeController : ControllerBase
    {
        private readonly IAppThemeService _service;

        public AppThemeController(IAppThemeService service)
        {
            _service = service;
        }

        // ── Queries ───────────────────────────────────────────────────────────────

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<AppThemeResponse>>> GetById(int id, CancellationToken ct)
        {
            var result = await _service.GetByIdAsync(id, ct);
            return result is null ? NotFound() : Ok(ApiResponse<AppThemeResponse>.Ok(result));
        }

        [HttpGet("default")]
        public async Task<ActionResult<ApiResponse<AppThemeResponse>>> GetDefault(CancellationToken ct)
        {
            var result = await _service.GetDefaultThemeAsync(ct);
            return result is null ? NotFound() : Ok(ApiResponse<AppThemeResponse>.Ok(result));
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<AppThemeResponse>>>> GetAll(CancellationToken ct)
        {
            var result = await _service.GetAllAsync(ct);
            return Ok(ApiResponse<IReadOnlyList<AppThemeResponse>>.Ok(result));
        }

        [HttpGet("active")]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<AppThemeResponse>>>> GetActive(CancellationToken ct)
        {
            var result = await _service.GetActiveAsync(ct);
            return Ok(ApiResponse<IReadOnlyList<AppThemeResponse>>.Ok(result));
        }

        // ── Commands ─────────────────────────────────────────────────────────────

        [HttpPost]
        public async Task<ActionResult<ApiResponse<AppThemeResponse>>> Create(
            [FromBody] CreateAppThemeRequest request, CancellationToken ct)
        {
            var result = await _service.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<AppThemeResponse>.Ok(result));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAppThemeRequest request, CancellationToken ct)
        {
            await _service.UpdateAsync(id, request, ct);
            return NoContent();
        }

        [HttpPatch("{id:int}/set-default")]
        public async Task<IActionResult> SetAsDefault(int id, CancellationToken ct)
        {
            await _service.SetAsDefaultAsync(id, ct);
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
