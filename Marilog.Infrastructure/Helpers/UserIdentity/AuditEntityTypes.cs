using Marilog.Domain.Common;
using Marilog.Infrastructure.Models.Identity;
using Marilog.Kernel.Enums;
using Microsoft.EntityFrameworkCore;

namespace Marilog.Infrastructure.Helpers.UserIdentity
{
    public static class AuditEntityTypes
    {
        public static EntityType Resolve(Type clrType)
            => Enum.TryParse<EntityType>(clrType.Name, out var result)
                ? result
                : EntityType.NONE;

        public static void ValidateAll(DbContext context)
        {
            var missing = context.Model.GetEntityTypes()
                .Select(t => t.ClrType)
                .Where(t => (typeof(Entity).IsAssignableFrom(t) && !t.IsAbstract) || t == typeof(User))
                .Where(t => Resolve(t) == EntityType.NONE)
                .Select(t => t.Name)
                .Distinct()
                .ToList();

            if (missing.Count > 0)
                throw new InvalidOperationException(
                    $"EntityType enum is missing values for: {string.Join(", ", missing)}");
        }
    }
}
