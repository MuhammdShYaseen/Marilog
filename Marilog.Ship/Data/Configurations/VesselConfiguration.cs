using Marilog.Domain.Entities.SystemEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marilog.Ship.Data.Configurations
{
    public class VesselConfiguration : IEntityTypeConfiguration<Vessel>
    {
        public void Configure(EntityTypeBuilder<Vessel> builder)
        {
            builder.ToTable("Vessels");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.VesselName).IsRequired().HasMaxLength(150);
            builder.Property(x => x.IMONumber).HasMaxLength(20);
            builder.Property(x => x.Notes).HasMaxLength(500);

            builder.HasIndex(x => x.IMONumber).IsUnique();
            builder.HasIndex(x => x.CompanyID);
            builder.HasIndex(x => x.FlagCountryID);

            builder.HasMany(x => x.Certificates)
                   .WithOne()
                   .HasForeignKey("VesselId")
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.FlagCountry)
                   .WithMany()
                   .HasForeignKey(x => x.FlagCountryID)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.VesselType)
                   .WithMany()
                   .HasForeignKey(x => x.VesselTypeId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.CrewContracts)
                   .WithOne(x => x.Vessel)
                   .HasForeignKey(x => x.VesselID)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}