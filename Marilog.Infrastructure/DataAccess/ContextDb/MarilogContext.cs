using Marilog.Domain.Common;
using Marilog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Marilog.Infrastructure.DataAccess.ContextDb
{
    public class MarilogContext : DbContext
    {
        public MarilogContext(DbContextOptions<MarilogContext> options) : base(options)
        {
        }
        // ── Core ────────────────────────────────────────────────────────────────
        public DbSet<Country> Countries => Set<Country>();
        public DbSet<Company> Companies => Set<Company>();
        public DbSet<Port> Ports => Set<Port>();
        public DbSet<Rank> Ranks => Set<Rank>();
        public DbSet<Vessel> Vessels => Set<Vessel>();
        public DbSet<Person> Persons => Set<Person>();
        public DbSet<CrewContract> CrewContracts => Set<CrewContract>();
        public DbSet<Voyage> Voyages => Set<Voyage>();
        public DbSet<VoyageStop> VoyageStops => Set<VoyageStop>();

        // ── Lookups ─────────────────────────────────────────────────────────────────
        public DbSet<Currency> Currencies => Set<Currency>();

        // ── Financial/Trade ─────────────────────────────────────────────────────
        public DbSet<Document> Documents => Set<Document>();
        public DbSet<DocumentItem> DocumentItems => Set<DocumentItem>();
        public DbSet<SwiftTransfer> SwiftTransfers => Set<SwiftTransfer>();
        public DbSet<Payment> Payments => Set<Payment>();

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
