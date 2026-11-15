using Marilog.Contracts.DTOs.LogDTOs;
using Marilog.Contracts.Interfaces.LogService;
using Microsoft.AspNetCore.Mvc;


namespace Marilog.Presentation.Controllers.SystemControllers
{
    [ApiController]
    [Route("api/logs")]
    [Produces("application/json")]
    public sealed class LogsController(ILogReaderService logReader, ILogger<LogsController> logger) : ControllerBase
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


        // DELETE /api/logs/files/{fileName}  ← today's files are refused
        [HttpDelete("files/{fileName}")]
        public async Task<IActionResult> DeleteFile(string fileName, CancellationToken ct)
        {
            await logReader.DeleteFileAsync(fileName, ct);
            logger.LogWarning("Log file {File} deleted by {User}", fileName, User.Identity?.Name);
            return NoContent();
        }

        // DELETE /api/logs/files  ← every file except today's
        [HttpDelete("files")]
        public async Task<IActionResult> DeleteOldFiles(CancellationToken ct)
        {
            var count = await logReader.DeleteOldFilesAsync(ct);
            logger.LogWarning("{Count} old log files deleted by {User}", count, User.Identity?.Name);
            return Ok(count);
        }
    }
}
