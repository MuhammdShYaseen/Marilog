using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Marilog.Ship.Data
{
    /// <summary>
    /// Everything the ship program keeps is in one folder, so a backup is a copy of that folder
    /// (with the program closed):
    ///   C:\Users\Public\Documents\Marilog Ship\ship.db   ← database
    ///   C:\Users\Public\Documents\Marilog Ship\Files\    ← attachments, previews, thumbnails
    /// Public Documents: visible in Explorer, the same for every Windows user on the PC, and not synced by OneDrive.
    /// </summary>
    public static class ShipDatabase
    {
        public static string DataFolder { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), "Marilog Ship");

        public static string FilePath { get; } = Path.Combine(DataFolder, "ship.db");

        public static string FilesFolder { get; } = Path.Combine(DataFolder, "Files");

        public static string ConnectionString => $"Data Source={FilePath}";

        public static void Initialize(IServiceProvider provider)
        {
            Directory.CreateDirectory(DataFolder);
            Directory.CreateDirectory(FilesFolder);

            using var scope = provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ShipContext>();
            db.Database.Migrate();
            ShipSeedData.Seed(db);
        }
    }
}