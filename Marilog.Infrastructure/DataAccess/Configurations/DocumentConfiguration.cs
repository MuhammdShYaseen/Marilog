using Marilog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace Marilog.Infrastructure.DataAccess.Configurations
{
    public class DocumentConfiguration : IEntityTypeConfiguration<Document>
    {
        public void Configure(EntityTypeBuilder<Document> builder)
        {
            builder.ToTable("Documents");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).UseIdentityColumn();
            builder.Property(x => x.DocNumber).IsRequired().HasMaxLength(50);
            builder.Property(x => x.DocTypeId).IsRequired();
            builder.Property(x => x.DocDate).IsRequired().HasColumnType("date");
            builder.Property(x => x.CurrencyId).IsRequired();
            builder.Property(x => x.TotalAmount).IsRequired().HasColumnType("decimal(18,2)");
            builder.Property(x => x.Reference).HasMaxLength(200);
            builder.Property(x => x.FilePath).HasMaxLength(500);
            builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
            builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

            // ── DocType (lookup FK) ───────────────────────────────────────────────
            builder.HasOne(x => x.DocType)
                   .WithMany()
                   .HasForeignKey(x => x.DocTypeId)
                   .OnDelete(DeleteBehavior.Restrict);

            // ── Currency ──────────────────────────────────────────────────────────
            builder.HasOne(x => x.Currency)
                   .WithMany()
                   .HasForeignKey(x => x.CurrencyId)
                   .OnDelete(DeleteBehavior.Restrict);

            // ── Supplier / Buyer ──────────────────────────────────────────────────
            builder.HasOne(x => x.Supplier)
                   .WithMany()
                   .HasForeignKey(x => x.SupplierId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Buyer)
                   .WithMany()
                   .HasForeignKey(x => x.BuyerId)
                   .OnDelete(DeleteBehavior.Restrict);

            // ── Vessel / Port ─────────────────────────────────────────────────────
            builder.HasOne(x => x.Vessel)
                   .WithMany()
                   .HasForeignKey(x => x.VesselId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Port)
                   .WithMany()
                   .HasForeignKey(x => x.PortId)
                   .OnDelete(DeleteBehavior.Restrict);

            // ── Self-reference (parent only — no navigation collection) ───────────
            builder.HasOne<Document>()
                   .WithMany()
                   .HasForeignKey(x => x.ParentDocumentId)
                   .OnDelete(DeleteBehavior.Restrict);

            // ── Owned children ────────────────────────────────────────────────────
            builder.HasMany(x => x.Items)
                   .WithOne()
                   .HasForeignKey(x => x.DocumentId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Payments)
                   .WithOne()
                   .HasForeignKey(x => x.DocumentId)
                   .OnDelete(DeleteBehavior.Cascade);

            // ── Indexes ───────────────────────────────────────────────────────────
            builder.HasIndex(x => x.DocNumber).IsUnique();
            builder.HasIndex(x => x.DocTypeId);
            builder.HasIndex(x => x.SupplierId);
            builder.HasIndex(x => x.BuyerId);
            builder.HasIndex(x => x.VesselId);
            builder.HasIndex(x => x.ParentDocumentId);
        }
    }
}
