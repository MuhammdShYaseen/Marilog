using Marilog.Application.DTOs.LogDTOs;
using Marilog.Application.Interfaces.LogService;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers.SystemControllers
{
    [ApiController]
    [Route("api/logs")]
    [Produces("application/json")]
    public sealed class LogsController(ILogReaderService logReader) : ControllerBase
    {
        // GET /api/logs?levels=ERR,WRN&search=KeyNotFound&page=1&pageSize=50
        [HttpGet]
        [ProducesResponseType<LogReadResult>(200)]
        public async Task<IActionResult> Query(
            [FromQuery] LogQuery query,
            CancellationToken ct)
        {
            var result = await logReader.QueryAsync(query, ct);
            return Ok(result);
        }

        // GET /api/logs/files  ← قائمة الملفات المتاحة
        [HttpGet("files")]
        [ProducesResponseType<IReadOnlyList<string>>(200)]
        public async Task<IActionResult> Files(CancellationToken ct)
        {
            var files = await logReader.GetAvailableFilesAsync(ct);
            return Ok(files);
        }

        // GET /api/logs/stats  ← إحصائيات سريعة بدون pagination
        [HttpGet("stats")]
        [ProducesResponseType<LogStats>(200)]
        public async Task<IActionResult> Stats(
            [FromQuery] string? fileName,
            CancellationToken ct)
        {
            var result = await logReader.QueryAsync(
                new LogQuery { FileName = fileName, PageSize = 500 }, ct);
            return Ok(result.Stats);
        }
    }
}
