using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.AuditDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.AuditLog;
using Marilog.Infrastructure.Helpers.UserIdentity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers.AuditLog
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = AuthPolicies.SuperAdmin)]
    public class AuditLogController : ControllerBase
    {
        private readonly IAuditLogService _service;

        public AuditLogController(IAuditLogService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResponse<AuditLogListItem>>> GetPaged(
            [FromQuery] PagedRequest paging, [FromQuery] AuditLogFilter filter, CancellationToken ct)
            => Ok(ApiResponse<PagedResponse<AuditLogListItem>>.Ok(
                await _service.GetPagedAsync(paging, filter, ct)));

        [HttpGet("{id:long}")]
        public async Task<ActionResult<AuditLogResponse>> GetById(long id, CancellationToken ct)
        {
            var result = await _service.GetByIdAsync(id, ct);
            if (result is null) return NotFound();
            return Ok(ApiResponse<AuditLogResponse>.Ok(result));
        }

        [HttpGet("settings")]
        public async Task<ActionResult<AuditSettingsOptions>> GetSettings(CancellationToken ct)
        => Ok(ApiResponse<AuditSettingsOptions>.Ok(await _service.GetSettingsAsync(ct)));

        [HttpPut("settings")]
        public async Task<IActionResult> UpdateSettings([FromBody] AuditSettingsOptions request, CancellationToken ct)
        {
            await _service.UpdateSettingsAsync(request, ct);
            return NoContent();
        }

        // No 'before' = delete everything older than the retention period
        [HttpDelete("purge")]
        public async Task<ActionResult<PurgeAuditLogsResponse>> Purge(
            [FromQuery] DateOnly? before, CancellationToken ct)
            => Ok(ApiResponse<PurgeAuditLogsResponse>.Ok(
                await _service.PurgeAsync(before, ct)));
    }
}
