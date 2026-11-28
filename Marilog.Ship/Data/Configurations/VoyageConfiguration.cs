using Marilog.Domain.Entities.SystemEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marilog.Ship.Data.Configurations
{
    // Only the voyage itself for now (Document.VoyageId points here).
    // Stops, bills of lading and contracts come with the voyages step.
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

            builder.Ignore(x => x.BillsOfLading);
            builder.Ignore(x => x.Stops);
            builder.Ignore(x => x.Contracts);

            builder.HasIndex(x => x.VesselID);
            builder.HasIndex(x => x.VoyageMonth);
            builder.HasIndex(x => x.Status);
        }
    }
}