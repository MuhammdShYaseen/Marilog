using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Marilog.Ship.Data
{
    public class ShipContextFactory : IDesignTimeDbContextFactory<ShipContext>
    {
        public ShipContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<ShipContext>()
                .UseSqlite("Data Source=design.db")
                .Options;

            return new ShipContext(options);
        }
    }
}