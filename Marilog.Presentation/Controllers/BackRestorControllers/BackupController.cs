using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.DataManagment;
using Marilog.Infrastructure.Helpers.UserIdentity;
using Marilog.Kernel.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace Marilog.Presentation.Controllers.BackRestorControllers
{
    [ApiController]
    [Route("api/backup")]
    [Authorize(Policy = AuthPolicies.SuperAdmin)]
    public class BackupController : ControllerBase
    {
        // Backups being built, or ready and waiting to be downloaded.
        // Kept in memory: a restart drops them, and their temp files with them.
        private static readonly ConcurrentDictionary<string, BackupJob> Jobs = new();
        private static readonly TimeSpan KeepFor = TimeSpan.FromHours(1);

        private readonly IFullBackupService _backupService;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BackupController> _logger;

        public BackupController(IFullBackupService backupService, IServiceScopeFactory scopeFactory, ILogger<BackupController> logger)
        {
            _backupService = backupService;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        // ── Backup in the background ─────────────────────────────

        [HttpPost("jobs")]
        public ActionResult<ApiResponse<BackupJobResponse>> StartBackup()
        {
            CleanUp();

            // One at a time: a second click, or coming back to the page, follows the running one
            var running = Jobs.Values.FirstOrDefault(j => j.Status == BackupJobStatus.Running);
            if (running is not null)
                return Ok(ApiResponse<BackupJobResponse>.Ok(running.ToResponse()));

            var job = new BackupJob(
                Convert.ToHexString(RandomNumberGenerator.GetBytes(32)),
                Path.GetTempFileName(),
                $"marilog-full-{DateTime.UtcNow:yyyyMMdd-HHmmss}{_backupService.FileExtension}");

            Jobs[job.Token] = job;
            _ = Task.Run(() => BuildAsync(job, _scopeFactory, _logger));

            return Ok(ApiResponse<BackupJobResponse>.Ok(job.ToResponse()));
        }

        [HttpGet("jobs/{token}")]
        public ActionResult<ApiResponse<BackupJobResponse>> GetBackup(string token)
        {
            return Jobs.TryGetValue(token, out var job)
                ? Ok(ApiResponse<BackupJobResponse>.Ok(job.ToResponse()))
                : NotFound(ApiResponse<BackupJobResponse>.Fail("This backup is no longer available. Create a new one."));
        }

        // Opened by the browser itself, which may not send the login cookie:
        // the random token is the key. It works once, and the file is deleted when the download ends.
        [HttpGet("download/{token}")]
        [AllowAnonymous]
        public IActionResult Download(string token)
        {
            if (!Jobs.TryGetValue(token, out var job) || job.Status != BackupJobStatus.Ready || !Jobs.TryRemove(token, out _))
                return NotFound(ApiResponse<object>.Fail("This backup link has expired. Create a new backup."));

            var stream = new FileStream(job.FilePath, FileMode.Open, FileAccess.Read, FileShare.None, 81920,
                FileOptions.Asynchronous | FileOptions.DeleteOnClose);

            return File(stream, "application/zip", job.FileName);
        }

        // ── Direct backup (old way) ──────────────────────────────

        [HttpGet("create")]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> CreateBackup(CancellationToken ct)
        {
            var fileName = $"marilog-full-{DateTime.UtcNow:yyyyMMdd-HHmmss}{_backupService.FileExtension}";

            // Write to a temp file first: an error then returns clean JSON, not a broken .zip
            var tempResultPath = Path.GetTempFileName();
            try
            {
                await using (var tempStream = new FileStream(tempResultPath, FileMode.Create, FileAccess.Write))
                {
                    await _backupService.CreateBackupAsync(tempStream, ct);
                }

                // Deleted when the download ends
                var result = new FileStream(tempResultPath, FileMode.Open, FileAccess.Read, FileShare.None, 81920,
                    FileOptions.Asynchronous | FileOptions.DeleteOnClose);
                return File(result, "application/octet-stream", fileName);
            }
            catch
            {
                TryDelete(tempResultPath);
                throw;
            }
        }

        // ── Restore ──────────────────────────────────────────────

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

        // ── Private Helpers ──────────────────────────────────────

        private static async Task BuildAsync(BackupJob job, IServiceScopeFactory scopeFactory, ILogger logger)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var service = scope.ServiceProvider.GetRequiredService<IFullBackupService>();

                await using (var file = new FileStream(job.FilePath, FileMode.Create, FileAccess.Write))
                {
                    await service.CreateBackupAsync(file);
                }

                job.SizeBytes = new FileInfo(job.FilePath).Length;
                job.Status = BackupJobStatus.Ready;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Background backup failed.");
                job.Error = ex.Message;
                job.Status = BackupJobStatus.Failed;
                TryDelete(job.FilePath);
            }
        }

        // Finished backups nobody downloaded within the hour
        private static void CleanUp()
        {
            foreach (var job in Jobs.Values.Where(j => j.Status != BackupJobStatus.Running && DateTime.UtcNow - j.CreatedAt > KeepFor))
            {
                if (Jobs.TryRemove(job.Token, out _))
                    TryDelete(job.FilePath);
            }
        }

        private static void TryDelete(string path)
        {
            try
            {
                if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
            }
            catch
            {
                // Still in use or already gone: the OS temp cleanup takes it
            }
        }

        private sealed class BackupJob(string token, string filePath, string fileName)
        {
            public string Token { get; } = token;
            public string FilePath { get; } = filePath;
            public string FileName { get; } = fileName;
            public DateTime CreatedAt { get; } = DateTime.UtcNow;

            // Written by the background task, read by requests
            public volatile BackupJobStatus Status = BackupJobStatus.Running;
            public long SizeBytes;
            public string? Error;

            public BackupJobResponse ToResponse() => new()
            {
                Token = Token,
                Status = Status,
                FileName = FileName,
                SizeBytes = SizeBytes,
                Error = Error
            };
        }
    }
}