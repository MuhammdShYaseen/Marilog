using Marilog.Domain.Entities.SystemEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marilog.Ship.Data.Configurations
{
    // Same as Marilog, without bills of lading (ignored in ShipContext).
    // Contracts are kept as Marilog has them (VoyageService reads them), with no charter-contract table on board.
    public class VoyageConfiguration : IEntityTypeConfiguration<Voyage>
    {
        public void Configure(EntityTypeBuilder<Voyage> builder)
        {
            builder.ToTable("Voyages");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.VoyageNumber).IsRequired().HasMaxLength(30);
            builder.Property(x => x.Status)
                   .IsRequired()
                   .HasConversion<string>()
                   .HasMaxLength(20);
            builder.Property(x => x.Notes).HasMaxLength(1000);

            builder.HasOne(x => x.Vessel)
                   .WithMany()
                   .HasForeignKey(x => x.VesselID)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.MasterContract)
                   .WithMany()
                   .HasForeignKey(x => x.MasterContractID)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Stops)
                   .WithOne()
                   .HasForeignKey(x => x.VoyageID)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.OwnsMany(x => x.Contracts, b =>
            {
                b.ToTable("VoyageContracts");
                b.WithOwner().HasForeignKey("VoyageID");
                b.HasKey("VoyageID", nameof(VoyageContract.ContractID));
                b.Property(p => p.ContractID).IsRequired().ValueGeneratedNever();
            });

            builder.Ignore(x => x.BillsOfLading);

            builder.HasIndex(x => x.VesselID);
            builder.HasIndex(x => x.VoyageMonth);
            builder.HasIndex(x => x.Status);
        }
    }
}