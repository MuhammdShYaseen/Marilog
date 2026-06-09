using Marilog.Domain.Entities.SystemEntities;
using Marilog.Kernel.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marilog.Infrastructure.DataAccess.Configurations
{
    public class BillOfLadingConfiguration : IEntityTypeConfiguration<BillOfLading>
    {
        public void Configure(EntityTypeBuilder<BillOfLading> builder)
        {
            builder.ToTable("BillsOfLading");
            builder.HasKey(b => b.Id);

            // ── Core Identity ───────────────────────────────────────────────
            builder.Property(b => b.BlNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(b => b.BlNumber)
                .IsUnique();

            builder.Property(b => b.BlType)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(b => b.IssuanceType)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            // ── الأطراف ─────────────────────────────────────────────────────
            builder.Property(b => b.ConsigneeToOrder)
                .HasMaxLength(200);

            builder.HasOne(b => b.ShipperCompany)
                .WithMany()
                .HasForeignKey(b => b.ShipperCompanyID)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.ConsigneeCompany)
                .WithMany()
                .HasForeignKey(b => b.ConsigneeCompanyID)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.NotifyPartyCompany)
                .WithMany()
                .HasForeignKey(b => b.NotifyPartyCompanyID)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.CarrierCompany)
                .WithMany()
                .HasForeignKey(b => b.CarrierCompanyID)
                .OnDelete(DeleteBehavior.Restrict);

            // ── الموانئ ─────────────────────────────────────────────────────
            builder.HasOne(b => b.PortOfLoading)
                .WithMany()
                .HasForeignKey(b => b.PortOfLoadingID)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.PortOfDischarge)
                .WithMany()
                .HasForeignKey(b => b.PortOfDischargeID)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.PlaceOfReceipt)
                .WithMany()
                .HasForeignKey(b => b.PlaceOfReceiptPortID)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.PlaceOfDelivery)
                .WithMany()
                .HasForeignKey(b => b.PlaceOfDeliveryPortID)
                .OnDelete(DeleteBehavior.Restrict);

            // ── البضاعة ─────────────────────────────────────────────────────
            builder.Property(b => b.CargoDescription)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(b => b.HsCode)
                .HasMaxLength(20);

            builder.Property(b => b.GrossWeightMT)
                .IsRequired()
                .HasColumnType("decimal(18,3)");

            builder.Property(b => b.VolumeM3)
                .HasColumnType("decimal(18,3)");

            builder.Property(b => b.PackageType)
                .HasMaxLength(50);

            builder.Property(b => b.MarksAndNumbers)
                .HasMaxLength(500);

            // ── الشروط المالية ──────────────────────────────────────────────
            builder.Property(b => b.FreightTerms)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(b => b.FreightAmount)
                .HasMaxLength(100);

            builder.Property(b => b.Incoterms)
                .HasMaxLength(10);

            // ── التواريخ ────────────────────────────────────────────────────
            builder.Property(b => b.IssueDate)
                .IsRequired();

            builder.Property(b => b.PlaceOfIssue)
                .HasMaxLength(100);

            // ── النسخ الأصلية ───────────────────────────────────────────────
            builder.Property(b => b.OriginalCopiesCount)
                .IsRequired()
                .HasDefaultValue(3);

            // ── MBL/HBL self-referencing ────────────────────────────────────
            builder.HasOne(b => b.MasterBl)
                .WithMany()
                .HasForeignKey(b => b.MasterBlID)
                .OnDelete(DeleteBehavior.Restrict);

            // ── Notes ───────────────────────────────────────────────────────
            builder.Property(b => b.Notes)
                .HasMaxLength(2000);

            // ── علاقة Voyage (Aggregate Root) ───────────────────────────────
            builder.HasOne(b => b.Voyage)
                .WithMany(v => v.BillsOfLading)
                .HasForeignKey(b => b.VoyageID)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(b => new { b.VoyageID, b.BlNumber }).IsUnique();
        }
    }
}