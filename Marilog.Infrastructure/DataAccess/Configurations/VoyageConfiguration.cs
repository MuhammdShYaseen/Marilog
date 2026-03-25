using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Marilog.Domain.Entities;

namespace Marilog.Infrastructure.DataAccess.Configurations
{
    public class VoyageConfiguration : IEntityTypeConfiguration<Voyage>
    {
        public void Configure(EntityTypeBuilder<Voyage> builder)
        {
            builder.ToTable("Voyages");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).UseIdentityColumn();
            builder.Property(x => x.VoyageNumber).IsRequired().HasMaxLength(30);
            builder.Property(x => x.VoyageMonth).IsRequired().HasColumnType("date");
            builder.Property(x => x.CargoType).HasMaxLength(150);
            builder.Property(x => x.CargoQuantityMT).HasColumnType("decimal(14,3)");
            builder.Property(x => x.Status)
                   .IsRequired()
                   .HasMaxLength(20)
                   .HasDefaultValue(VoyageStatus.PLANNED)
                   .HasConversion<string>();
            builder.Property(x => x.PreviousMasterBalance).HasColumnType("decimal(12,2)").HasDefaultValue(0m);
            builder.Property(x => x.CashOnBoard).HasColumnType("decimal(12,2)").HasDefaultValue(0m);
            builder.Property(x => x.CigarettesOnBoard).HasColumnType("decimal(12,2)").HasDefaultValue(0m);
            builder.Property(x => x.Notes).HasMaxLength(1000);
            builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
            builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_Voyages_Status",
                "Status IN ('PLANNED','UNDERWAY','COMPLETED','CANCELLED')"));

            builder.HasOne(x => x.MasterContract)
                   .WithMany()
                   .HasForeignKey(x => x.MasterContractID)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.DeparturePort)
                   .WithMany()
                   .HasForeignKey(x => x.DeparturePortID)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ArrivalPort)
                   .WithMany()
                   .HasForeignKey(x => x.ArrivalPortID)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Stops)
                   .WithOne()
                   .HasForeignKey(x => x.VoyageID)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.VesselID);
            builder.HasIndex(x => x.VoyageMonth);
            builder.HasIndex(x => x.Status);
        }
    }
}
