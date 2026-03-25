using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Marilog.Domain.Entities;

namespace Marilog.Infrastructure.DataAccess.Configurations
{
    public class VesselConfiguration : IEntityTypeConfiguration<Vessel>
    {
        public void Configure(EntityTypeBuilder<Vessel> builder)
        {
            builder.ToTable("Vessels");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).UseIdentityColumn();
            builder.Property(x => x.VesselName).IsRequired().HasMaxLength(150);
            builder.Property(x => x.IMONumber).HasMaxLength(20);
            builder.Property(x => x.GrossTonnage).HasColumnType("decimal(12,2)");
            builder.Property(x => x.Notes).HasMaxLength(500);
            builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
            builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");
            builder.HasIndex(x => x.IMONumber).IsUnique();

            builder.HasOne(x => x.FlagCountry)
                   .WithMany()
                   .HasForeignKey(x => x.FlagCountryID)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.CrewContracts)
                   .WithOne(x => x.Vessel)
                   .HasForeignKey(x => x.VesselID)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.CompanyID);
            builder.HasIndex(x => x.FlagCountryID);
        }
    }
}
