using Marilog.Domain.Entities.SystemEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marilog.Ship.Data.Configurations
{
    public class DocumentConfiguration : IEntityTypeConfiguration<Document>
    {
        public void Configure(EntityTypeBuilder<Document> builder)
        {
            builder.ToTable("Documents");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.DocNumber).IsRequired().HasMaxLength(50);
            builder.Property(x => x.Reference).HasMaxLength(200);
            builder.Property(x => x.SearchVector).HasMaxLength(1000);
            builder.Property(x => x.Side).HasConversion<int>();

            builder.HasOne(x => x.DocType)
                   .WithMany()
                   .HasForeignKey(x => x.DocTypeId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Currency)
                   .WithMany()
                   .HasForeignKey(x => x.CurrencyId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Supplier)
                   .WithMany()
                   .HasForeignKey(x => x.SupplierId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Buyer)
                   .WithMany()
                   .HasForeignKey(x => x.BuyerId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Vessel)
                   .WithMany()
                   .HasForeignKey(x => x.VesselId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Port)
                   .WithMany()
                   .HasForeignKey(x => x.PortId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Voyage)
                   .WithMany()
                   .HasForeignKey(x => x.VoyageId)
                   .OnDelete(DeleteBehavior.SetNull);

            // Parent only — no navigation collection (same as Marilog).
            builder.HasOne<Document>()
                   .WithMany()
                   .HasForeignKey(x => x.ParentDocumentId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Items)
                   .WithOne()
                   .HasForeignKey(x => x.DocumentId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Payments)
                   .WithOne(p => p.Document)
                   .HasForeignKey(x => x.DocumentId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Adjustments)
                   .WithOne()
                   .HasForeignKey(x => x.DocumentId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.DocNumber).IsUnique();
            builder.HasIndex(x => x.DocTypeId);
            builder.HasIndex(x => x.SupplierId);
            builder.HasIndex(x => x.BuyerId);
            builder.HasIndex(x => x.VesselId);
            builder.HasIndex(x => x.ParentDocumentId);
            builder.HasIndex(x => x.SearchVector);
            builder.HasIndex(x => new { x.IsActive, x.DocDate });
        }
    }
}