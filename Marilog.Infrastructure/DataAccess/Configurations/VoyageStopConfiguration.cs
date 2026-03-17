using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Marilog.Domain.Entities;

namespace Marilog.Infrastructure.DataAccess.Configurations
{
    public class VoyageStopConfiguration : IEntityTypeConfiguration<VoyageStop>
    {
        public void Configure(EntityTypeBuilder<VoyageStop> builder)
        {
            builder.ToTable("VoyageStops");
            builder.HasKey(x => x.StopID);
            builder.Property(x => x.StopID).UseIdentityColumn();
            builder.Property(x => x.StopOrder).IsRequired();
            builder.Property(x => x.PurposeOfCall).HasMaxLength(200);
            builder.Property(x => x.Notes).HasMaxLength(500);

            builder.HasIndex(x => new { x.VoyageID, x.StopOrder }).IsUnique();

            builder.HasOne(x => x.Port)
                   .WithMany()
                   .HasForeignKey(x => x.PortID)
                   .IsRequired(false)
                   .OnDelete(DeleteBehavior.SetNull);

            // ── Matching query filter to avoid global filter warning ─────────
            builder.HasQueryFilter(x => !x.Port!.IsDeleted);

            builder.HasIndex(x => x.PortID);
        }
    }
}