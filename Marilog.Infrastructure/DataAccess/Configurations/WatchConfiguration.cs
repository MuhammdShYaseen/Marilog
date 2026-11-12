using Marilog.Domain.Entities.NotifiEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marilog.Infrastructure.DataAccess.Configurations
{
    public class WatchConfiguration : IEntityTypeConfiguration<Watch>
    {
        public void Configure(EntityTypeBuilder<Watch> builder)
        {
            builder.ToTable("Watches");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Title).HasMaxLength(300).IsRequired();

            builder.HasIndex(x => new { x.UserId, x.EntityType, x.EntityId }).IsUnique();
            // The job matches AuditLog.EntityId (a Guid) against this
            builder.HasIndex(x => x.EntityGuid);

            builder.Property(x => x.CreatedAt)
                   .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
        }
    }
}