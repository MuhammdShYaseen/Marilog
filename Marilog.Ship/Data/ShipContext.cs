using System.Reflection;
using Marilog.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Marilog.Ship.Data
{
    public class ShipContext : DbContext
    {
        public ShipContext(DbContextOptions<ShipContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ShipContext).Assembly);
            ApplyGlobalFilters(modelBuilder);
        }

        // Same as MarilogContext: unique Guid index + soft-delete filter on every Entity.
        private static void SetSoftDeleteFilter<T>(ModelBuilder builder) where T : Entity
        {
            builder.Entity<T>().HasQueryFilter(e => !e.IsDeleted);
        }

        private static void ApplyGlobalFilters(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (!typeof(Entity).IsAssignableFrom(entityType.ClrType)) continue;

                modelBuilder.Entity(entityType.ClrType)
                    .HasIndex(nameof(Entity.Guid))
                    .IsUnique();

                var method = typeof(ShipContext)
                    .GetMethod(nameof(SetSoftDeleteFilter), BindingFlags.NonPublic | BindingFlags.Static)!
                    .MakeGenericMethod(entityType.ClrType);

                method.Invoke(null, new object[] { modelBuilder });
            }
        }
    }
}