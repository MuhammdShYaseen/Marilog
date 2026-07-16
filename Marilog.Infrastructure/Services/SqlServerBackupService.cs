using Marilog.Application.Interfaces.DataManagment;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Marilog.Infrastructure.Services
{
    public class SqlServerBackupService : IDatabaseBackupService
    {
        private readonly string _connectionString;
        private readonly string _databaseName;
        private readonly string _stagingPath; // مسار يقدر السيرفر SQL Server يوصله (لوكال أو UNC)
        private readonly ILogger<SqlServerBackupService> _logger;

        public string ProviderName => "SqlServer";
        public string FileExtension => ".bak";

        public SqlServerBackupService(IConfiguration config, ILogger<SqlServerBackupService> logger)
        {
            _logger = logger;

            // نفضّل connection string مخصص بصلاحيات backup/restore، ولو مش موجود نرجع للـ Default
            // (لكن الأفضل دايماً تضيف "BackupOperations" منفصل بصلاحيات أعلى، متل ما اتفقنا سابقاً)
            _connectionString = config.GetConnectionString("BackupOperations")
                ?? config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "No connection string named 'BackupOperations' or 'DefaultConnection' was found in appsettings.json");

            _databaseName = new SqlConnectionStringBuilder(_connectionString).InitialCatalog;

            if (string.IsNullOrWhiteSpace(_databaseName))
                throw new InvalidOperationException("InitialCatalog was not found in the backup operations connection string");

            // fail-fast: if the config key is missing, throw immediately at application startup
            // instead of failing later inside Path.Combine on the first real user request
            _stagingPath = config["Backup:SqlServerStagingPath"]
                ?? throw new InvalidOperationException(
                    "The key 'Backup:SqlServerStagingPath' was not found in appsettings.json. " +
                    "It must point to a path accessible by the SQL Server engine (local or UNC).");

            EnsureStagingDirectoryExists();
        }

        private void EnsureStagingDirectoryExists()
        {
            try
            {
                if (!Directory.Exists(_stagingPath))
                {
                    Directory.CreateDirectory(_stagingPath);
                    _logger.LogInformation("Created backup staging directory: {Path}", _stagingPath);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Could not create or access the staging directory specified by Backup:SqlServerStagingPath = '{_stagingPath}'. " +
                    "Make sure the path is valid and that the account running the API has write permission to it.", ex);
            }
        }

        public async Task CreateBackupAsync(Stream destination, CancellationToken ct = default)
        {
            var serverPath = Path.Combine(_stagingPath, $"{_databaseName}_{DateTime.UtcNow:yyyyMMddHHmmss}.bak");
            _logger.LogInformation("Starting backup of database {Database} to {Path}", _databaseName, serverPath);

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(ct);

            // Express Edition does not support BACKUP COMPRESSION, so we detect the feature dynamically instead of assuming it
            var supportsCompression = await SupportsBackupCompressionAsync(connection, ct);
            var withClause = supportsCompression ? "WITH FORMAT, COMPRESSION" : "WITH FORMAT";

            if (!supportsCompression)
                _logger.LogInformation(
                    "The current SQL Server Edition does not support BACKUP COMPRESSION (likely Express). " +
                    "The backup will be created without compression, resulting in a larger file size than usual.");

            await using (var cmd = new SqlCommand(
                $"BACKUP DATABASE [{_databaseName}] TO DISK = @path {withClause}", connection))
            {
                cmd.Parameters.AddWithValue("@path", serverPath);
                cmd.CommandTimeout = 0; // قواعد البيانات الكبيرة محتاجة وقت
                await cmd.ExecuteNonQueryAsync(ct);
            }

            if (!File.Exists(serverPath))
                throw new InvalidOperationException(
                    $"BACKUP DATABASE completed without an exception, but the resulting file was not found at: {serverPath}");

            try
            {
                await using var file = new FileStream(serverPath, FileMode.Open, FileAccess.Read,
                    FileShare.Read, 81920, useAsync: true);
                await file.CopyToAsync(destination, ct);

                _logger.LogInformation("Backup of database {Database} completed successfully", _databaseName);
            }
            finally
            {
                if (File.Exists(serverPath)) File.Delete(serverPath);
            }
        }

        public async Task RestoreBackupAsync(Stream source, CancellationToken ct = default)
        {
            EnsureStagingDirectoryExists(); // safety net in case the directory was deleted after application startup

            var serverPath = Path.Combine(_stagingPath, $"restore_{Guid.NewGuid():N}.bak");
            _logger.LogWarning("Starting restore of database {Database} from uploaded file", _databaseName);

            await using (var file = new FileStream(serverPath, FileMode.Create, FileAccess.Write,
                FileShare.None, 81920, useAsync: true))
            {
                await source.CopyToAsync(file, ct);
            }

            var builder = new SqlConnectionStringBuilder(_connectionString)
            {
                InitialCatalog = "master"
            };

            await using var connection = new SqlConnection(builder.ConnectionString);
            await connection.OpenAsync(ct);

            try
            {
                await ExecAsync(connection, $"ALTER DATABASE [{_databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE", ct);

                // بدل ما نحدد أسماء الملفات hardcoded، نقرأها ديناميكياً من الـ backup نفسه
                var (dataLogical, logLogical) = await GetLogicalFileNamesAsync(connection, serverPath, ct);

                if (dataLogical is null || logLogical is null)
                    throw new InvalidOperationException(
                        "Could not read the logical names of the data/log files from the backup file. " +
                        "Make sure the uploaded file is a valid .bak file actually produced by BACKUP DATABASE.");

                var dataDir = await GetServerPropertyAsync(connection, "InstanceDefaultDataPath", ct);
                var logDir = await GetServerPropertyAsync(connection, "InstanceDefaultLogPath", ct);

                var restoreSql = $@"
                RESTORE DATABASE [{_databaseName}] FROM DISK = @path
                WITH REPLACE,
                MOVE '{dataLogical}' TO '{Path.Combine(dataDir, _databaseName + ".mdf")}',
                MOVE '{logLogical}' TO '{Path.Combine(logDir, _databaseName + "_log.ldf")}'";

                await using var cmd = new SqlCommand(restoreSql, connection);
                cmd.Parameters.AddWithValue("@path", serverPath);
                cmd.CommandTimeout = 0;
                await cmd.ExecuteNonQueryAsync(ct);

                _logger.LogWarning("Restore of database {Database} completed successfully", _databaseName);
            }
            finally
            {
                // always attempt to bring the database back to MULTI_USER, whether the restore succeeded or failed
                try
                {
                    await ExecAsync(connection, $"ALTER DATABASE [{_databaseName}] SET MULTI_USER", ct);
                }
                catch (Exception ex)
                {
                    // if even reverting to MULTI_USER fails, this must be surfaced loudly since it means the database is stuck
                    _logger.LogCritical(ex,
                        "Failed to revert database {Database} to MULTI_USER mode after a restore attempt. " +
                        "The database may be stuck in SINGLE_USER mode and requires immediate manual intervention.", _databaseName);
                }

                if (File.Exists(serverPath)) File.Delete(serverPath);
            }
        }

        private static async Task ExecAsync(SqlConnection c, string sql, CancellationToken ct)
        {
            await using var cmd = new SqlCommand(sql, c);
            await cmd.ExecuteNonQueryAsync(ct);
        }

        private static async Task<(string? Data, string? Log)> GetLogicalFileNamesAsync(
            SqlConnection c, string backupPath, CancellationToken ct)
        {
            await using var cmd = new SqlCommand("RESTORE FILELISTONLY FROM DISK = @path", c);
            cmd.Parameters.AddWithValue("@path", backupPath);
            await using var reader = await cmd.ExecuteReaderAsync(ct);

            string? data = null, log = null;
            while (await reader.ReadAsync(ct))
            {
                var name = reader.GetString(reader.GetOrdinal("LogicalName"));
                var type = reader.GetString(reader.GetOrdinal("Type"));
                if (type == "D") data = name;
                if (type == "L") log = name;
            }
            return (data, log);
        }

        private static async Task<string> GetServerPropertyAsync(SqlConnection c, string prop, CancellationToken ct)
        {
            await using var cmd = new SqlCommand("SELECT CAST(SERVERPROPERTY(@prop) AS NVARCHAR(4000))", c);
            cmd.Parameters.AddWithValue("@prop", prop);
            var result = await cmd.ExecuteScalarAsync(ct);
            return result as string
                ?? throw new InvalidOperationException($"SERVERPROPERTY('{prop}') returned NULL or an unexpected type");
        }

        private static async Task<bool> SupportsBackupCompressionAsync(SqlConnection c, CancellationToken ct)
        {
            // EngineEdition: 4 = Express (لا يدعم Compression)
            // غيرها (2=Standard, 3=Enterprise, 5=Azure SQL DB, 8=Azure Managed Instance...) بتدعمها
            await using var cmd = new SqlCommand("SELECT SERVERPROPERTY('EngineEdition')", c);
            var result = await cmd.ExecuteScalarAsync(ct);
            var engineEdition = Convert.ToInt32(result);
            return engineEdition != 4;
        }
    }
}