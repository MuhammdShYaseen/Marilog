using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

namespace Marilog.Ship.Data
{
    public static class ShipDatabase
    {
        public static string FilePath { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Marilog.Ship", "ship.db");

        public static string ConnectionString => $"Data Source={FilePath}";

        public static void Initialize(IServiceProvider provider)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);

            using var scope = provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ShipContext>();
            db.Database.Migrate();
            ShipSeedData.Seed(db);
        }
    }
}