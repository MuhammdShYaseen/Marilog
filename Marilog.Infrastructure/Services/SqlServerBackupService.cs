using Marilog.Application.Interfaces.DataManagment;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Marilog.Infrastructure.Services
{
    public class SqlServerBackupService : IDatabaseBackupService
    {
        private readonly string _connectionString;
        private readonly string _databaseName;
        private readonly string _stagingPath; // مسار يقدر السيرفر SQL Server يوصله (لوكال أو UNC)

        public string ProviderName => "SqlServer";
        public string FileExtension => ".bak";

        public SqlServerBackupService(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection")!;
            _databaseName = new SqlConnectionStringBuilder(_connectionString).InitialCatalog;
            _stagingPath = config["Backup:SqlServerStagingPath"]!;
        }

        public async Task CreateBackupAsync(Stream destination, CancellationToken ct = default)
        {
            var serverPath = Path.Combine(_stagingPath, $"{_databaseName}_{DateTime.UtcNow:yyyyMMddHHmmss}.bak");

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(ct);

            await using (var cmd = new SqlCommand(
                $"BACKUP DATABASE [{_databaseName}] TO DISK = @path WITH FORMAT, COMPRESSION", connection))
            {
                cmd.Parameters.AddWithValue("@path", serverPath);
                cmd.CommandTimeout = 0; // قواعد البيانات الكبيرة محتاجة وقت
                await cmd.ExecuteNonQueryAsync(ct);
            }

            try
            {
                await using var file = new FileStream(serverPath, FileMode.Open, FileAccess.Read,
                    FileShare.Read, 81920, useAsync: true);
                await file.CopyToAsync(destination, ct);
            }
            finally
            {
                if (File.Exists(serverPath)) File.Delete(serverPath);
            }
        }

        public async Task RestoreBackupAsync(Stream source, CancellationToken ct = default)
        {
            var serverPath = Path.Combine(_stagingPath, $"restore_{Guid.NewGuid():N}.bak");

            await using (var file = new FileStream(serverPath, FileMode.Create, FileAccess.Write,
                FileShare.None, 81920, useAsync: true))
            {
                await source.CopyToAsync(file, ct);
            }
            //var masterConnStr = _connectionString.Replace(_databaseName, "master");
            var builder = new SqlConnectionStringBuilder(_connectionString)
            {
                InitialCatalog = "master"
            };

            var masterConnStr = builder.ConnectionString;
            await using var connection = new SqlConnection(masterConnStr);
            await connection.OpenAsync(ct);

            try
            {
                await ExecAsync(connection, $"ALTER DATABASE [{_databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE", ct);

                // بدل ما نحدد أسماء الملفات hardcoded، نقرأها ديناميكياً من الـ backup نفسه
                var (dataLogical, logLogical) = await GetLogicalFileNamesAsync(connection, serverPath, ct);
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
            }
            finally
            {
                // دايماً نرجع الداتابيز MULTI_USER، سواء نجح أو فشل الـ Restore
                await ExecAsync(connection, $"ALTER DATABASE [{_databaseName}] SET MULTI_USER", ct);
                if (File.Exists(serverPath)) File.Delete(serverPath);
            }
        }

        private static async Task ExecAsync(SqlConnection c, string sql, CancellationToken ct)
        {
            await using var cmd = new SqlCommand(sql, c);
            await cmd.ExecuteNonQueryAsync(ct);
        }

        private static async Task<(string Data, string Log)> GetLogicalFileNamesAsync(
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
            return (data!, log!);
        }

        private static async Task<string> GetServerPropertyAsync(SqlConnection c, string prop, CancellationToken ct)
        {
            await using var cmd = new SqlCommand($"SELECT SERVERPROPERTY('{prop}')", c);
            return (string)(await cmd.ExecuteScalarAsync(ct))!;
        }
    }
}
