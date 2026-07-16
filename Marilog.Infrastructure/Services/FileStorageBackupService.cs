using System.IO.Compression;
using Marilog.Application.Interfaces.DataManagment;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Marilog.Infrastructure.Services
{
    public class FileStorageBackupService : IFileStorageBackupService
    {
        private readonly string _basePath; // نفس FileStorage:BasePath المستخدم بـ LocalFileStorageProvider
        private readonly ILogger<FileStorageBackupService> _logger;

        public string FileExtension => ".zip";

        public FileStorageBackupService(IConfiguration config, ILogger<FileStorageBackupService> logger)
        {
            _logger = logger;

            _basePath = config["FileStorage:BasePath"]
                ?? throw new InvalidOperationException("FileStorage:BasePath not configured.");

            if (!Directory.Exists(_basePath))
                Directory.CreateDirectory(_basePath);
        }

        public async Task CreateBackupAsync(Stream destination, CancellationToken ct = default)
        {
            var tempZipPath = Path.Combine(Path.GetTempPath(), $"marilog-files-{DateTime.UtcNow:yyyyMMddHHmmss}.zip");
            _logger.LogInformation("Starting files backup from {BasePath}", _basePath);

            try
            {
                // ZipFile.CreateFromDirectory محتاج ملف فعلي كـ destination (يعمل seek داخلياً)، فما نقدر نضغط مباشرة على الـ response stream
                ZipFile.CreateFromDirectory(_basePath, tempZipPath, CompressionLevel.Optimal, includeBaseDirectory: false);

                await using var zipStream = new FileStream(tempZipPath, FileMode.Open, FileAccess.Read,
                    FileShare.Read, 81920, useAsync: true);
                await zipStream.CopyToAsync(destination, ct);

                _logger.LogInformation("Files backup completed successfully");
            }
            finally
            {
                if (File.Exists(tempZipPath)) File.Delete(tempZipPath);
            }
        }

        public async Task RestoreBackupAsync(Stream source, CancellationToken ct = default)
        {
            var tempZipPath = Path.Combine(Path.GetTempPath(), $"restore-{Guid.NewGuid():N}.zip");
            var tempExtractPath = Path.Combine(Path.GetTempPath(), $"restore-extract-{Guid.NewGuid():N}");

            await using (var fs = new FileStream(tempZipPath, FileMode.Create, FileAccess.Write,
                FileShare.None, 81920, useAsync: true))
            {
                await source.CopyToAsync(fs, ct);
            }

            try
            {
                // نفك الضغط بمجلد مؤقت منفصل أولاً، عشان لو فشل الاستخراج ما نكون مسحنا الملفات الحالية أصلاً
                ZipFile.ExtractToDirectory(tempZipPath, tempExtractPath, overwriteFiles: true);

                _logger.LogWarning("Restoring files into {BasePath} (full replace)", _basePath);

                var oldPath = _basePath + "_old_" + Guid.NewGuid().ToString("N");

                if (Directory.Exists(_basePath))
                    Directory.Move(_basePath, oldPath); // نبعد الملفات الحالية بدل ما نمسحها فوراً

                try
                {
                    Directory.Move(tempExtractPath, _basePath); // نحط الملفات المستعادة بمكانها
                }
                catch
                {
                    // لو فشل النقل، نرجع الملفات القديمة لمكانها الأصلي بدل ما نخسرها
                    if (Directory.Exists(oldPath) && !Directory.Exists(_basePath))
                        Directory.Move(oldPath, _basePath);
                    throw;
                }

                if (Directory.Exists(oldPath))
                    Directory.Delete(oldPath, recursive: true);

                _logger.LogWarning("Files restore completed successfully");
            }
            finally
            {
                if (File.Exists(tempZipPath)) File.Delete(tempZipPath);
                if (Directory.Exists(tempExtractPath)) Directory.Delete(tempExtractPath, recursive: true);
            }
        }
    }
}