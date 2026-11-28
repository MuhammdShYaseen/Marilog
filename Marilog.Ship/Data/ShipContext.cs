using System.Reflection;
using Marilog.Application.Interfaces.Events;
using Marilog.Domain.Common;
using Marilog.Domain.Entities.SystemEntities;
using Marilog.Ship.Domain.Entities;
using Marilog.Ship.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

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

            // Not on the ship. Ignored for the whole model, before any configuration: when EF
            // discovers Payment and Voyage by convention it would follow these and keep them
            // (SwiftTransfer → Bank ..., Voyage → BillOfLading → ...).
            modelBuilder.Ignore<SwiftTransfer>();
            modelBuilder.Ignore<BillOfLading>();

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ShipContext).Assembly);
            ApplyGlobalFilters(modelBuilder);
        }

        // Same as MarilogContext: domain events are sent only after the save succeeded.
        public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            await EnsureNotInClosedMgaAsync(ct);

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

        // ── Closed MGA lock ──────────────────────────────────────────────────────
        // A record dated on or before the last day of the last closed MGA cannot be added,
        // changed or deleted: it is in an account already sent to the office. Checked here,
        // once, so Marilog's services (documents, payments, voyages) are covered too.
        // The way out is reopening the last MGA with an unlock code.
        private async Task EnsureNotInClosedMgaAsync(CancellationToken ct)
        {
            var dates = new List<DateOnly>();
            var documentIds = new List<int>();

            foreach (var entry in ChangeTracker.Entries())
            {
                if (!IsRealChange(entry)) continue;

                switch (entry.Entity)
                {
                    case OwnerRemittance:
                        dates.AddRange(DatesOf(entry, nameof(OwnerRemittance.Date)));
                        break;
                    case CrewWithdrawal:
                        dates.AddRange(DatesOf(entry, nameof(CrewWithdrawal.Date)));
                        break;
                    case CrewExtra:
                        dates.AddRange(DatesOf(entry, nameof(CrewExtra.Date)));
                        break;
                    case Payment:
                        dates.AddRange(DatesOf(entry, nameof(Payment.PaymentDate)));
                        break;
                    case VoyageStop:
                        // A stop still in port (no departure) can always be finished.
                        dates.AddRange(DatesOf(entry, nameof(VoyageStop.DepartureDate)));
                        break;
                    case Document document when entry.State == EntityState.Modified
                        && (IsChanged(entry, nameof(Document.CurrencyId)) || IsChanged(entry, nameof(Document.DocTypeId))):
                        // Its payments would move to another currency or out of / into page 3.
                        documentIds.Add(document.Id);
                        break;
                }
            }

            if (dates.Count == 0 && documentIds.Count == 0) return;

            var lockedTo = await Set<Mga>().AsNoTracking()
                .Where(x => x.Status == MgaStatus.Closed)
                .OrderByDescending(x => x.ToDate)
                .Select(x => (DateOnly?)x.ToDate)
                .FirstOrDefaultAsync(ct);
            if (lockedTo is null) return;

            var locked = dates.Any(d => d <= lockedTo)
                || (documentIds.Count > 0 && await Set<Payment>().AnyAsync(
                        p => documentIds.Contains(p.DocumentId) && p.PaymentDate <= lockedTo, ct));

            if (locked)
                throw new InvalidOperationException(
                    $"Records dated up to {lockedTo:dd.MM.yyyy} are in a closed MGA and cannot be changed. " +
                    "Reopen the last MGA with an unlock code from the office.");
        }

        // Marilog services call Update() on the whole document, so "Modified" alone is not a change.
        private static bool IsRealChange(EntityEntry entry) => entry.State switch
        {
            EntityState.Added or EntityState.Deleted => true,
            EntityState.Modified => entry.Properties.Any(p =>
                p.Metadata.Name != nameof(Entity.UpdatedAt) && !Equals(p.OriginalValue, p.CurrentValue)),
            _ => false
        };

        private static bool IsChanged(EntityEntry entry, string property)
        {
            var p = entry.Property(property);
            return !Equals(p.OriginalValue, p.CurrentValue);
        }

        // The old date (moving a record out of a closed MGA) and the new one (moving it in).
        private static IEnumerable<DateOnly> DatesOf(EntityEntry entry, string property)
        {
            var p = entry.Property(property);
            var values = entry.State switch
            {
                EntityState.Added => new[] { p.CurrentValue },
                EntityState.Deleted => new[] { p.OriginalValue },
                _ => new[] { p.OriginalValue, p.CurrentValue }
            };

            foreach (var value in values)
            {
                if (value is DateOnly date) yield return date;
                else if (value is DateTime dateTime) yield return DateOnly.FromDateTime(dateTime);
            }
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