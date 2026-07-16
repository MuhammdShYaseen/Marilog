using Marilog.Application.DTOs.DataManagment;
using Marilog.Application.Interfaces.DataManagment;
using Microsoft.Extensions.Logging;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;

namespace Marilog.Infrastructure.Services
{
    public class FullBackupService : IFullBackupService
    {
        // أسماء الـ entries جوا الـ zip الموحد - ثابتة عشان الاستعادة تلاقيهم بشكل موثوق بدون تخمين
        private const string DatabaseEntryName = "database";
        private const string FilesEntryName = "files";
        private const string ManifestEntryName = "manifest.json";

        private readonly IDatabaseBackupService _databaseBackupService;
        private readonly IFileStorageBackupService _fileStorageBackupService;
        private readonly ISchemaVersionProvider _schemaVersionProvider;
        private readonly ILogger<FullBackupService> _logger;

        public string FileExtension => ".zip";

        public FullBackupService(
            IDatabaseBackupService databaseBackupService,
            IFileStorageBackupService fileStorageBackupService,
            ISchemaVersionProvider schemaVersionProvider,
            ILogger<FullBackupService> logger)
        {
            _databaseBackupService = databaseBackupService;
            _fileStorageBackupService = fileStorageBackupService;
            _schemaVersionProvider = schemaVersionProvider;
            _logger = logger;
        }

        public async Task CreateBackupAsync(Stream destination, CancellationToken ct = default)
        {
            var tempDbPath = Path.Combine(Path.GetTempPath(), $"fb-db-{Guid.NewGuid():N}{_databaseBackupService.FileExtension}");
            var tempFilesPath = Path.Combine(Path.GetTempPath(), $"fb-files-{Guid.NewGuid():N}{_fileStorageBackupService.FileExtension}");
            var tempCombinedPath = Path.Combine(Path.GetTempPath(), $"fb-combined-{Guid.NewGuid():N}.zip");

            try
            {
                // 1) باك اب قاعدة البيانات
                _logger.LogInformation("Full backup: creating database backup");
                await using (var dbStream = new FileStream(tempDbPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true))
                {
                    await _databaseBackupService.CreateBackupAsync(dbStream, ct);
                }

                // 2) باك اب الملفات
                _logger.LogInformation("Full backup: creating files backup");
                await using (var filesStream = new FileStream(tempFilesPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true))
                {
                    await _fileStorageBackupService.CreateBackupAsync(filesStream, ct);
                }

                // 2.5) بناء manifest.json: توقيت، نسخة الـ schema، وchecksum لكل من الملفين
                // لازم يصير هون، بعد ما صار عندنا الملفين النهائيين تماماً وقبل الدمج بالأرشيف
                _logger.LogInformation("Full backup: computing manifest (schema version + checksums)");
                var manifest = new BackupManifest
                {
                    CreatedAtUtc = DateTime.UtcNow,
                    SchemaVersion = await _schemaVersionProvider.GetCurrentVersionAsync(ct),
                    DatabaseProvider = _databaseBackupService.ProviderName,
                    DatabaseSha256 = await ComputeSha256Async(tempDbPath, ct),
                    FilesSha256 = await ComputeSha256Async(tempFilesPath, ct)
                };

                // 3) دمج الاثنين بملف zip واحد + manifest.json
                _logger.LogInformation("Full backup: merging database, files, and manifest into a single archive");
                using (var archive = ZipFile.Open(tempCombinedPath, ZipArchiveMode.Create))
                {
                    archive.CreateEntryFromFile(tempDbPath, DatabaseEntryName + _databaseBackupService.FileExtension, CompressionLevel.NoCompression);
                    archive.CreateEntryFromFile(tempFilesPath, FilesEntryName + _fileStorageBackupService.FileExtension, CompressionLevel.NoCompression);

                    var manifestEntry = archive.CreateEntry(ManifestEntryName, CompressionLevel.NoCompression);
                    await using var manifestStream = manifestEntry.Open();
                    await JsonSerializer.SerializeAsync(manifestStream, manifest, cancellationToken: ct);
                }
                // ملاحظة: CompressionLevel.NoCompression هون لأنه المحتوى مضغوط أصلاً (backup مضغوط + zip الملفات)،
                // إعادة ضغطه بيضيع وقت بدون أي فايدة حقيقية بالحجم

                await using (var combinedStream = new FileStream(tempCombinedPath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true))
                {
                    await combinedStream.CopyToAsync(destination, ct);
                }

                _logger.LogInformation("Full backup completed successfully");
            }
            finally
            {
                if (File.Exists(tempDbPath)) File.Delete(tempDbPath);
                if (File.Exists(tempFilesPath)) File.Delete(tempFilesPath);
                if (File.Exists(tempCombinedPath)) File.Delete(tempCombinedPath);
            }
        }

        public async Task RestoreBackupAsync(Stream source, CancellationToken ct = default)
        {
            var tempCombinedPath = Path.Combine(Path.GetTempPath(), $"fb-restore-combined-{Guid.NewGuid():N}.zip");
            var tempDbPath = Path.Combine(Path.GetTempPath(), $"fb-restore-db-{Guid.NewGuid():N}{_databaseBackupService.FileExtension}");
            var tempFilesPath = Path.Combine(Path.GetTempPath(), $"fb-restore-files-{Guid.NewGuid():N}{_fileStorageBackupService.FileExtension}");

            await using (var fs = new FileStream(tempCombinedPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true))
            {
                await source.CopyToAsync(fs, ct);
            }

            try
            {
                // 1) استخراج ملفي الـ database والـ files، ومعهم manifest.json، من الأرشيف الموحد
                _logger.LogInformation("Full restore: extracting database, files, and manifest entries from the combined archive");
                BackupManifest manifest;
                using (var archive = ZipFile.OpenRead(tempCombinedPath))
                {
                    var dbEntry = FindEntry(archive, DatabaseEntryName)
                        ?? throw new InvalidOperationException($"No database entry found inside the uploaded archive (expected an entry starting with '{DatabaseEntryName}').");

                    var filesEntry = FindEntry(archive, FilesEntryName)
                        ?? throw new InvalidOperationException($"No files entry found inside the uploaded archive (expected an entry starting with '{FilesEntryName}').");

                    var manifestEntry = archive.GetEntry(ManifestEntryName)
                        ?? throw new InvalidOperationException("Archive is missing manifest.json — this is not a valid full backup produced by this system.");

                    dbEntry.ExtractToFile(tempDbPath, overwrite: true);
                    filesEntry.ExtractToFile(tempFilesPath, overwrite: true);

                    await using var manifestStream = manifestEntry.Open();
                    manifest = await JsonSerializer.DeserializeAsync<BackupManifest>(manifestStream, cancellationToken: ct)
                        ?? throw new InvalidOperationException("Could not parse manifest.json from the archive.");
                }

                // 1.5) التحقق من الـ manifest قبل أي عملية استعادة فعلية - لو في مشكلة لازم تنكشف هون
                // وليس بمنتصف عملية الاستعادة على قاعدة بيانات حية
                _logger.LogInformation(
                    "Full restore: validating manifest (created {CreatedAtUtc}, schema {SchemaVersion}, provider {Provider})",
                    manifest.CreatedAtUtc, manifest.SchemaVersion, manifest.DatabaseProvider);

                if (!string.Equals(manifest.DatabaseProvider, _databaseBackupService.ProviderName, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException(
                        $"This backup was created for provider '{manifest.DatabaseProvider}', but the current system is configured for '{_databaseBackupService.ProviderName}'. " +
                        "Restoring across providers is not supported by this service.");

                var actualDbHash = await ComputeSha256Async(tempDbPath, ct);
                if (!string.Equals(actualDbHash, manifest.DatabaseSha256, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Database backup integrity check failed (checksum mismatch). The archive may be corrupted or was tampered with.");

                var actualFilesHash = await ComputeSha256Async(tempFilesPath, ct);
                if (!string.Equals(actualFilesHash, manifest.FilesSha256, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Files backup integrity check failed (checksum mismatch). The archive may be corrupted or was tampered with.");

                var currentSchemaVersion = await _schemaVersionProvider.GetCurrentVersionAsync(ct);
                if (!string.Equals(currentSchemaVersion, manifest.SchemaVersion, StringComparison.OrdinalIgnoreCase))
                    _logger.LogWarning(
                        "Schema version mismatch: backup was created with '{BackupSchema}' but the current application schema is '{CurrentSchema}'. " +
                        "Proceeding, but data inconsistency is possible — run pending migrations before or after restore as appropriate.",
                        manifest.SchemaVersion, currentSchemaVersion);

                // 2) استعادة قاعدة البيانات أولاً
                _logger.LogWarning("Full restore: restoring database");
                await using (var dbStream = new FileStream(tempDbPath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true))
                {
                    await _databaseBackupService.RestoreBackupAsync(dbStream, ct);
                }

                // 3) استعادة الملفات بعدها
                _logger.LogWarning("Full restore: restoring files");
                await using (var filesStream = new FileStream(tempFilesPath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true))
                {
                    await _fileStorageBackupService.RestoreBackupAsync(filesStream, ct);
                }

                _logger.LogWarning("Full restore completed successfully");
            }
            finally
            {
                if (File.Exists(tempCombinedPath)) File.Delete(tempCombinedPath);
                if (File.Exists(tempDbPath)) File.Delete(tempDbPath);
                if (File.Exists(tempFilesPath)) File.Delete(tempFilesPath);
            }
        }

        private static ZipArchiveEntry? FindEntry(ZipArchive archive, string namePrefix) =>
            archive.Entries.FirstOrDefault(e => e.Name.StartsWith(namePrefix, StringComparison.OrdinalIgnoreCase));

        private static async Task<string> ComputeSha256Async(string filePath, CancellationToken ct)
        {
            await using var stream = File.OpenRead(filePath);
            var hash = await SHA256.HashDataAsync(stream, ct);
            return Convert.ToHexString(hash);
        }
    }
}