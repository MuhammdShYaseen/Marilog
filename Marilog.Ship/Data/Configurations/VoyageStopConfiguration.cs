using Marilog.Domain.Entities.SystemEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marilog.Ship.Data.Configurations
{
    // Port calls of a voyage — MGA page 1 (ship's movements).
    public class VoyageStopConfiguration : IEntityTypeConfiguration<VoyageStop>
    {
        public void Configure(EntityTypeBuilder<VoyageStop> builder)
        {
            builder.ToTable("VoyageStops");
            builder.HasKey(x => x.StopID);
            builder.Property(x => x.StopOrder).IsRequired();
            builder.Property(x => x.PurposeOfCall).HasMaxLength(200);
            builder.Property(x => x.Notes).HasMaxLength(500);

            builder.HasOne(x => x.Port)
                   .WithMany()
                   .HasForeignKey(x => x.PortID)
                   .IsRequired(false)
                   .OnDelete(DeleteBehavior.Restrict);

            // Matching query filter (same as Marilog) to avoid the global filter warning.
            builder.HasQueryFilter(x => !x.Port!.IsDeleted);

            builder.HasMany(x => x.Cargo)
                   .WithOne()
                   .HasForeignKey(x => x.StopID)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.VoyageID, x.StopOrder }).IsUnique();
            builder.HasIndex(x => x.PortID);
        }
    }
}