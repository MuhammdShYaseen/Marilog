using Marilog.Domain.Common;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Marilog.Infrastructure.DataAccess.ContextDb
{
    public class MarilogContext : DbContext
    {
        public MarilogContext(DbContextOptions<MarilogContext> options) : base(options)
        {
        }


        private static void SetSoftDeleteFilter<T>(ModelBuilder builder)
         where T : Entity
        {
            builder.Entity<T>()
                   .HasQueryFilter(e => !e.IsDeleted);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
            ApplyGlobalFilters(modelBuilder);
        }

        private static void ApplyGlobalFilters(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(Entity).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .HasIndex(nameof(Entity.Guid))
                        .IsUnique();
                }

                if (typeof(Entity).IsAssignableFrom(entityType.ClrType))
                {
                    var method = typeof(MarilogContext)
                        .GetMethod(nameof(SetSoftDeleteFilter),
                            BindingFlags.NonPublic | BindingFlags.Static)!
                        .MakeGenericMethod(entityType.ClrType);

                    method.Invoke(null, new object[] { modelBuilder });
                }
            }
        }

    }
}
