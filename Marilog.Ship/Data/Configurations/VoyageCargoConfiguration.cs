using Marilog.Domain.Entities.SystemEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marilog.Ship.Data.Configurations
{
    // Same as Marilog. Not entered on board for now, but VoyageService reads it.
    // ContractID stays a plain column (no charter-contract table on board).
    public class VoyageCargoConfiguration : IEntityTypeConfiguration<VoyageCargo>
    {
        public void Configure(EntityTypeBuilder<VoyageCargo> builder)
        {
            builder.ToTable("VoyageCargo");
            builder.HasKey(x => x.VoyageCargoID);
            builder.Property(x => x.Operation).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(x => x.Unit).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(x => x.Notes).HasMaxLength(500);

            builder.HasOne(x => x.CargoType)
                   .WithMany()
                   .HasForeignKey(x => x.CargoTypeID)
                   .OnDelete(DeleteBehavior.Restrict);

            // Matching query filter (same as Marilog).
            builder.HasQueryFilter(x => !x.CargoType.IsDeleted);

            builder.OwnsOne(x => x.Bulk, b =>
            {
                b.ToTable("VoyageCargoBulk");
                b.WithOwner().HasForeignKey("VoyageCargoID");
            });

            builder.OwnsOne(x => x.General, b =>
            {
                b.ToTable("VoyageCargoGeneral");
                b.WithOwner().HasForeignKey("VoyageCargoID");
                b.Property(p => p.PackageType).HasMaxLength(50);
            });
        }
    }
}