using System.Reflection;
using Marilog.Application.Interfaces.Events;
using Marilog.Domain.Common;
using Marilog.Domain.Entities.SystemEntities;
using Microsoft.EntityFrameworkCore;

namespace Marilog.Ship.Data
{
    public class ShipContext : DbContext
    {
        private readonly IEventDispatcher? _dispatcher;

        // The dispatcher is missing only in ShipContextFactory (design time, migrations).
        public ShipContext(DbContextOptions<ShipContext> options, IEventDispatcher? dispatcher = null) : base(options)
        {
            _dispatcher = dispatcher;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Not on the ship (yet). Ignored for the whole model, before any configuration: when EF
            // discovers Payment and Voyage by convention it would follow these and keep them
            // (SwiftTransfer → Bank ..., Voyage → BillOfLading, VoyageStop → VoyageCargo → CargoType).
            modelBuilder.Ignore<SwiftTransfer>();
            modelBuilder.Ignore<BillOfLading>();
            modelBuilder.Ignore<VoyageStop>();
            modelBuilder.Ignore<VoyageCargo>();
            modelBuilder.Ignore<VoyageContract>();
            modelBuilder.Ignore<CargoType>();
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ShipContext).Assembly);
            ApplyGlobalFilters(modelBuilder);
        }

        // Same as MarilogContext: domain events are sent only after the save succeeded.
        public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            var entitiesWithEvents = ChangeTracker
                .Entries<Entity>()
                .Where(e => e.Entity.DomainEvents.Any())
                .Select(e => e.Entity)
                .ToList();

            var events = entitiesWithEvents
                .SelectMany(e => e.DomainEvents)
                .ToList();

            var result = await base.SaveChangesAsync(ct);

            entitiesWithEvents.ForEach(e => e.ClearDomainEvents());

            if (_dispatcher is not null)
                foreach (var domainEvent in events)
                    await _dispatcher.EnqueueAsync(domainEvent, ct);

            return result;
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