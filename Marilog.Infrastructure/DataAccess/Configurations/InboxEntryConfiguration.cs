using Marilog.Domain.Entities.SystemEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marilog.Infrastructure.DataAccess.Configurations
{
    public class InboxEntryConfiguration : IEntityTypeConfiguration<InboxEntry>
    {
        public void Configure(EntityTypeBuilder<InboxEntry> builder)
        {
            builder.ToTable("InboxEntries");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Number)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(x => x.Source)
                .IsRequired();

            // Primitive collection — stored as a JSON column (EF Core 8+)
            builder.Property(x => x.VesselIds);

            builder.HasIndex(x => x.Number).IsUnique();
            builder.HasIndex(x => new { x.Year, x.Sequence }).IsUnique();
            builder.HasIndex(x => new { x.IsProcessed, x.IsCancelled });
            builder.HasIndex(x => x.ReceivedDate);
        }
    }
}
