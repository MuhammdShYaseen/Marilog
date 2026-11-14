using Marilog.Domain.Entities.NotifiEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marilog.Infrastructure.DataAccess.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("Notifications");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Text).HasMaxLength(500).IsRequired();
            builder.Property(x => x.DedupKey).HasMaxLength(200).IsRequired();

            // One notification per event per user, even if the job reads a log twice
            builder.HasIndex(x => new { x.UserId, x.DedupKey }).IsUnique();
            // The bell's unread count
            builder.HasIndex(x => new { x.UserId, x.ReadAt });

            builder.Property(x => x.CreatedAt)
                   .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
            builder.Property(x => x.ReadAt)
                   .HasConversion(v => v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTime?)null);
            builder.Property(x => x.EmailedAt)
                   .HasConversion(v => v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTime?)null);
        }
    }
}