using Marilog.Application.Interfaces.DataManagment;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers.DatabaseController
{
    [ApiController]
    [Route("api/backup")]
    public class DatabaseBackupController : ControllerBase
    {
        private readonly IDatabaseBackupService _backupService;

        public DatabaseBackupController(IDatabaseBackupService backupService)
            => _backupService = backupService;

        [HttpPost("create")]
        [DisableRequestSizeLimit]
        public async Task CreateBackup(CancellationToken ct)
        {
            var fileName = $"marilog-{DateTime.UtcNow:yyyyMMdd-HHmmss}{_backupService.FileExtension}";
            Response.Headers.Append("Content-Disposition", $"attachment; filename=\"{fileName}\"");
            Response.ContentType = "application/octet-stream";

            // كتابة مباشرة على الـ response stream — بدون تحميل الملف بالذاكرة، تماماً متل تحميل ملفات الـ OCR
            await _backupService.CreateBackupAsync(Response.Body, ct);
        }

        [HttpPost("restore")]
        [DisableRequestSizeLimit]
        [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
        public async Task<IActionResult> RestoreBackup(CancellationToken ct)
        {
            var form = await Request.ReadFormAsync(ct);
            var file = form.Files.FirstOrDefault();
            if (file is null || file.Length == 0)
                return BadRequest("no backup attached");

            await using var uploadStream = file.OpenReadStream();
            await _backupService.RestoreBackupAsync(uploadStream, ct);

            return Ok(new { message = "restored" });
        }
    }
}
