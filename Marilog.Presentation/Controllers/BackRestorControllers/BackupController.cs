using Marilog.Application.Interfaces.DataManagment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers.BackRestorControllers
{
    [ApiController]
    [Route("api/backup")]
    [Authorize(Roles = "Admin")] // عملية حساسة جداً (قاعدة بيانات + كل الملفات) - ما تفتحها لأي دور
    public class BackupController : ControllerBase
    {
        private readonly IFullBackupService _backupService;

        public BackupController(IFullBackupService backupService)
            => _backupService = backupService;

        [HttpPost("create")]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> CreateBackup(CancellationToken ct)
        {
            var fileName = $"marilog-full-{DateTime.UtcNow:yyyyMMdd-HHmmss}{_backupService.FileExtension}";

            // نكتب لملف مؤقت أولاً بدل ما نكتب على الـ response مباشرة، عشان لو صار استثناء
            // ما نكون خربنا الـ response بـ headers "attachment/.zip" على محتوى خطأ فعلياً JSON
            // (نفس المشكلة يلي واجهناها سابقاً مع ملف الـ .bak المزيف)
            var tempResultPath = Path.GetTempFileName();
            try
            {
                await using (var tempStream = new FileStream(tempResultPath, FileMode.Create, FileAccess.Write))
                {
                    await _backupService.CreateBackupAsync(tempStream, ct);
                }

                // وصلنا هون يعني نجحت العملية فعلاً، هلق منزل الملف
                return PhysicalFile(tempResultPath, "application/octet-stream", fileName);
            }
            catch
            {
                if (System.IO.File.Exists(tempResultPath))
                    System.IO.File.Delete(tempResultPath);
                throw; // يوصل لـ ErrorHandlerMiddleware ويرجع JSON نظيف بامتداد ونوع محتوى صحيحين
            }
        }

        [HttpPost("restore")]
        [DisableRequestSizeLimit]
        [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
        public async Task<IActionResult> RestoreBackup(CancellationToken ct)
        {
            var form = await Request.ReadFormAsync(ct);
            var file = form.Files.FirstOrDefault();
            if (file is null || file.Length == 0)
                return BadRequest(new { message = "No backup file attached." });

            await using var uploadStream = file.OpenReadStream();
            await _backupService.RestoreBackupAsync(uploadStream, ct);

            return Ok(new { message = "Full restore (database + files) completed successfully." });
        }
    }
}