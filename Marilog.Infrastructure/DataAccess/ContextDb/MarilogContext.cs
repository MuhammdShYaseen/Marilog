using Marilog.Application.Interfaces.Events;
using Marilog.Domain.Common;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Marilog.Infrastructure.DataAccess.ContextDb
{
    public class MarilogContext : DbContext
    {
        private readonly IEventDispatcher _dispatcher;
        public MarilogContext(DbContextOptions<MarilogContext> options, IEventDispatcher dispatcher) : base(options)
        {
            _dispatcher = dispatcher;
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

            // Domain events are dispatched only after successful persistence
            var result = await base.SaveChangesAsync(ct);

            entitiesWithEvents.ForEach(e => e.ClearDomainEvents());

            foreach (var domainEvent in events)
                await _dispatcher.EnqueueAsync(domainEvent, ct);

            return result;
        }
    }
}
