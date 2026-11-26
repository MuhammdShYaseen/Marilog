using Marilog.Ship.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marilog.Ship.Data.Configurations
{
    public class ShipSetupConfiguration : IEntityTypeConfiguration<ShipSetup>
    {
        public void Configure(EntityTypeBuilder<ShipSetup> builder)
        {
            builder.ToTable("ShipSetup");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.DefaultWagesPaidBy)
                   .IsRequired()
                   .HasConversion<string>()
                   .HasMaxLength(20);

            builder.HasOne(x => x.Vessel)
                   .WithMany()
                   .HasForeignKey(x => x.VesselId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}