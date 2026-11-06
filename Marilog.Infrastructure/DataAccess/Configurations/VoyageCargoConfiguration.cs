using Marilog.Domain.Entities.SystemEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marilog.Infrastructure.DataAccess.Configurations
{
    public class VoyageCargoConfiguration : IEntityTypeConfiguration<VoyageCargo>
    {
        public void Configure(EntityTypeBuilder<VoyageCargo> builder)
        {
            builder.ToTable("VoyageCargo");
            builder.HasKey(x => x.VoyageCargoID);
            builder.Property(x => x.VoyageCargoID).UseIdentityColumn();

            builder.Property(x => x.Operation).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(x => x.Quantity).IsRequired().HasColumnType("decimal(14,3)");
            builder.Property(x => x.Unit).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(x => x.WeightMt).HasColumnType("decimal(14,3)");
            builder.Property(x => x.Notes).HasMaxLength(500);

            builder.HasOne(x => x.CargoType)
                   .WithMany()
                   .HasForeignKey(x => x.CargoTypeID)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<AContract>()
                   .WithMany()
                   .HasForeignKey(x => x.ContractID)
                   .OnDelete(DeleteBehavior.Restrict);

            // ── Matching query filter (same pattern as VoyageStop → Port) ──
            builder.HasQueryFilter(x => !x.CargoType.IsDeleted);

            // ── Extensions: each in its own table, keyed by VoyageCargoID ──
            builder.OwnsOne(x => x.Bulk, b =>
            {
                b.ToTable("VoyageCargoBulk");
                b.WithOwner().HasForeignKey("VoyageCargoID");
                b.Property(p => p.DraftSurveyMt).HasColumnType("decimal(14,3)");
                b.Property(p => p.ShoreFigureMt).HasColumnType("decimal(14,3)");
            });

            builder.OwnsOne(x => x.General, b =>
            {
                b.ToTable("VoyageCargoGeneral");
                b.WithOwner().HasForeignKey("VoyageCargoID");
                b.Property(p => p.PackageType).HasMaxLength(50);
                b.Property(p => p.VolumeCbm).HasColumnType("decimal(14,3)");
            });
        }
    }
}