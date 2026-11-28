using System.IO.Compression;
using Marilog.Ship.Data;
using Microsoft.Data.Sqlite;

namespace Marilog.Ship.Services.DataBackup
{
    /// <summary>
    /// One zip with everything on board: a consistent copy of ship.db (VACUUM INTO works while the
    /// program is using the database) and every other file in the data folder (attached files, MGA PDFs).
    /// Restore = close the program and unzip into the data folder.
    /// </summary>
    public static class ShipBackup
    {
        private const string FolderSettingFile = "backup-folder.txt";

        public static string DefaultFolder => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Marilog Ship Backups");

        /// <summary>The folder used last time (kept in the data folder), else Documents\Marilog Ship Backups.</summary>
        public static string GetFolder()
        {
            var setting = Path.Combine(ShipDatabase.DataFolder, FolderSettingFile);
            var saved = File.Exists(setting) ? File.ReadAllText(setting).Trim() : null;
            return string.IsNullOrWhiteSpace(saved) ? DefaultFolder : saved;
        }

        public static void SetFolder(string folder) =>
            File.WriteAllText(Path.Combine(ShipDatabase.DataFolder, FolderSettingFile), folder.Trim());

        /// <summary>The newest backup in the folder, if any.</summary>
        public static FileInfo? Latest(string folder) =>
            Directory.Exists(folder)
                ? new DirectoryInfo(folder).GetFiles("MarilogShip_*.zip").OrderByDescending(x => x.LastWriteTime).FirstOrDefault()
                : null;

        /// <summary>Creates MarilogShip_[vessel]_[yyyyMMdd_HHmm].zip in the folder and returns its path.</summary>
        public static async Task<string> CreateAsync(string folder, string? vesselName, CancellationToken ct = default)
        {
            Directory.CreateDirectory(folder);
            var zipPath = Path.Combine(folder, $"MarilogShip_{SafeName(vesselName ?? "Ship")}_{DateTime.Now:yyyyMMdd_HHmm}.zip");

            var temp = Path.Combine(Path.GetTempPath(), "Marilog Ship", "backup-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(temp);
            try
            {
                var dbCopy = Path.Combine(temp, "ship.db");
                await using (var connection = new SqliteConnection(ShipDatabase.ConnectionString))
                {
                    await connection.OpenAsync(ct);
                    await using var command = connection.CreateCommand();
                    command.CommandText = "VACUUM INTO $path";
                    command.Parameters.AddWithValue("$path", dbCopy);
                    await command.ExecuteNonQueryAsync(ct);
                }

                // Zipping the attached files can take a while: off the UI thread.
                await Task.Run(() =>
                {
                    if (File.Exists(zipPath)) File.Delete(zipPath);
                    using var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create);

                    zip.CreateEntryFromFile(dbCopy, "ship.db", CompressionLevel.Optimal);

                    foreach (var file in Directory.EnumerateFiles(ShipDatabase.DataFolder, "*", SearchOption.AllDirectories))
                    {
                        var relative = Path.GetRelativePath(ShipDatabase.DataFolder, file);
                        if (relative.StartsWith("ship.db", StringComparison.OrdinalIgnoreCase)) continue;   // the copy above
                        zip.CreateEntryFromFile(file, relative.Replace('\\', '/'), CompressionLevel.Optimal);
                    }
                }, ct);

                return zipPath;
            }
            finally
            {
                try { Directory.Delete(temp, recursive: true); } catch { /* temp folder, not important */ }
            }
        }

        private static string SafeName(string text)
        {
            var invalid = Path.GetInvalidFileNameChars();
            return new string(text.Trim().Select(c => invalid.Contains(c) || c == ' ' ? '_' : c).ToArray());
        }
    }
}