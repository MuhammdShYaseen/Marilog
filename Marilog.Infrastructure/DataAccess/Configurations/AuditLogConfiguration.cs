using Marilog.Domain.Entities.UsersLog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marilog.Infrastructure.DataAccess.Configurations
{
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.ToTable("AuditLogs");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.UserName).HasMaxLength(256);
            builder.HasIndex(x => new { x.EntityType, x.EntityId });
            builder.HasIndex(x => x.OccurredAt);
            builder.Property(l => l.OccurredAt)
                   .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
        }
    }
}
