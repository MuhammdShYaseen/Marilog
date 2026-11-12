using DocumentFormat.OpenXml.Spreadsheet;
using Marilog.Application.Interfaces.UserIdentity;
using Marilog.Domain.Common;
using Marilog.Domain.Entities.SystemEntities;
using Marilog.Domain.Entities.UsersLog;
using Marilog.Infrastructure.Helpers.UserIdentity;
using Marilog.Infrastructure.Models.Identity;
using Marilog.Kernel.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Marilog.Infrastructure.DataAccess.Interceptors
{
    public sealed class AuditSaveChangesInterceptor : SaveChangesInterceptor
    {
        private static readonly HashSet<string> IgnoredUserProperties = new()
        {
            nameof(User.PasswordHash),
            nameof(User.SecurityStamp),
            nameof(User.ConcurrencyStamp),
            nameof(User.AccessFailedCount),
            nameof(User.LockoutEnd)
        };

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
            Converters = { new JsonStringEnumConverter() }
        };

        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<AuditSaveChangesInterceptor> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly List<AuditLog> _ownLogs = new();
        public AuditSaveChangesInterceptor(
            ICurrentUserService currentUser,
            ILogger<AuditSaveChangesInterceptor> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _currentUser = currentUser;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            AddAuditLogs(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            AddAuditLogs(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void AddAuditLogs(DbContext? context)
        {
            // No HTTP request means a background service or startup code: not audited
            if (context is null || _httpContextAccessor.HttpContext is null || !_currentUser.IsAuthenticated) return;

            context.ChangeTracker.DetectChanges();

            // Drop logs this interceptor added for an earlier save that failed.
            // Logs added by other code (e.g. a purge record) are left alone.
            foreach (var stale in _ownLogs)
            {
                var staleEntry = context.Entry(stale);
                if (staleEntry.State == EntityState.Added)
                    staleEntry.State = EntityState.Detached;
            }
            _ownLogs.Clear();

            var logs = new List<AuditLog>();

            var entries = context.ChangeTracker.Entries().ToList();
            foreach (var entry in entries)
            {
                if (entry.Entity is AuditLog or CommentRead) continue;
                if (entry.State is EntityState.Detached or EntityState.Unchanged) continue;

                Guid entityId;
                EntityType? type = null;
                var asParentUpdate = false;

                if (entry.Entity is Entity entity) entityId = entity.Guid;
                else if (entry.Entity is User user) entityId = user.Id;
                else if (FindParent(entries, entry) is { } parent) (entityId, type, asParentUpdate) = parent;
                else continue;

                var log = BuildLog(entry, entityId, type, asParentUpdate);
                if (log is not null) logs.Add(log);
            }

            if (logs.Count > 0)
            {
                context.Set<AuditLog>().AddRange(logs);
                _ownLogs.AddRange(logs);
            }
        }

        private AuditLog? BuildLog(EntityEntry entry, Guid entityId, EntityType? type = null, bool asParentUpdate = false)
        {
            var properties = entry.Properties
                .Where(p => !IsIgnored(entry, p))
                .ToList();

            AuditOperation operation;
            string? before = null;
            string? after = null;

            switch (entry.State)
            {
                case EntityState.Added:
                    operation = AuditOperation.Create;
                    after = Serialize(entry, properties, current: true);
                    break;

                case EntityState.Deleted:
                    operation = AuditOperation.Delete;
                    before = Serialize(entry, properties, current: false);
                    break;

                case EntityState.Modified:
                    {
                        var changed = properties
                            .Where(p => p.Metadata.Name != nameof(Entity.UpdatedAt))
                            .Where(p => !ValuesEqual(p.OriginalValue, p.CurrentValue))
                            .ToList();

                        if (changed.Count == 0) return null;

                        operation = IsSoftDelete(entry) ? AuditOperation.Delete : AuditOperation.Update;
                        before = Serialize(entry, changed, current: false);
                        after = Serialize(entry, changed, current: true);
                        break;
                    }

                default:
                    return null;
            }

            // A value with no type of its own is a change to its parent
            if (asParentUpdate) operation = AuditOperation.Update;
            var isAuthenticated = _currentUser.IsAuthenticated;

            return AuditLog.Create(
                type ?? AuditEntityTypes.Resolve(entry.Metadata.ClrType),
                entityId,
                operation,
                isAuthenticated ? _currentUser.UserId : null,
                isAuthenticated ? _currentUser.UserName : null,
                before,
                after);
        }

        // A tracked record that isn't an Entity (a document item, a voyage stop or its cargo,
        // an owned value like a bank account or the laytime terms) is logged under its nearest
        // parent Entity's Guid, so the parent's history and its followers see the change.
        // Type: the first one on the way up that EntityType names (DocumentItem, VoyageCargo...).
        // A plain value with no type of its own is logged as an update of the parent.
        private static (Guid Id, EntityType Type, bool AsParentUpdate)? FindParent(List<EntityEntry> entries, EntityEntry entry)
        {
            EntityType? childType = null;
            var current = entry;

            for (var depth = 0; depth < 5 && current is not null; depth++)
            {
                if (current.Entity is Entity parent)
                    return childType is { } type
                        ? (parent.Guid, type, false)
                        : (parent.Guid, AuditEntityTypes.Resolve(current.Metadata.ClrType), true);

                var resolved = AuditEntityTypes.Resolve(current.Metadata.ClrType);
                if (childType is null && resolved != EntityType.NONE) childType = resolved;

                current = FindPrincipal(entries, current);
            }

            return null;
        }

        // The tracked parent: the owner of an owned value, or the record whose collection holds this one.
        private static EntityEntry? FindPrincipal(List<EntityEntry> entries, EntityEntry entry)
        {
            foreach (var fk in entry.Metadata.GetForeignKeys())
            {
                if (!fk.IsOwnership && fk.PrincipalToDependent is not { IsCollection: true }) continue;

                var keyValues = fk.Properties
                    .Select(p => entry.State == EntityState.Deleted
                        ? entry.Property(p.Name).OriginalValue
                        : entry.Property(p.Name).CurrentValue)
                    .ToArray();
                if (keyValues.Any(v => v is null)) continue;

                var principal = entries.FirstOrDefault(e =>
                    fk.PrincipalEntityType.ClrType.IsAssignableFrom(e.Metadata.ClrType)
                    && fk.PrincipalKey.Properties.Select(p => e.Property(p.Name).CurrentValue).SequenceEqual(keyValues));

                if (principal is not null) return principal;
            }

            return null;
        }
        private static bool IsIgnored(EntityEntry entry, PropertyEntry property)
        {
            // Identity Id on a new row is a temporary value until the database assigns it
            if (property.IsTemporary) return true;

            if (property.Metadata.PropertyInfo?.GetCustomAttribute<AuditIgnoreAttribute>() is not null)
                return true;

            if (entry.Entity is User && IgnoredUserProperties.Contains(property.Metadata.Name))
                return true;

            return false;
        }

        private static bool IsSoftDelete(EntityEntry entry)
        {
            if (entry.Entity is not Entity) return false;

            var isDeleted = entry.Property(nameof(Entity.IsDeleted));
            return isDeleted.OriginalValue is false && isDeleted.CurrentValue is true;
        }

        private static bool ValuesEqual(object? a, object? b)
            => a is byte[] x && b is byte[] y
                ? x.AsSpan().SequenceEqual(y)
                : Equals(a, b);

        private string Serialize(EntityEntry entry, List<PropertyEntry> properties, bool current)
        {
            var values = properties.ToDictionary(
                p => p.Metadata.Name,
                p => current ? p.CurrentValue : p.OriginalValue);

            try
            {
                return JsonSerializer.Serialize(values, JsonOptions);
            }
            catch (Exception ex)
            {
                // Never let audit serialization block the business save
                _logger.LogError(ex, "Audit serialization failed for {EntityType}", entry.Metadata.ClrType.Name);
                return "{\"_auditError\":\"serialization failed\"}";
            }
        }
    }
}