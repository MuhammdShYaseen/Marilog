using Marilog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Infrastructure.DataAccess.Configurations
{
    public class DocumentTypeConfiguration : IEntityTypeConfiguration<DocumentType>
    {
        public void Configure(EntityTypeBuilder<DocumentType> builder)
        {
            builder.ToTable("DocumentTypes");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).UseIdentityColumn();
            builder.Property(x => x.Code).IsRequired().HasMaxLength(30);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
            builder.Property(x => x.SortOrder).IsRequired().HasDefaultValue(0);
            builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
            builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

            builder.HasIndex(x => x.Code).IsUnique();

            // ── Seed well-known types ─────────────────────────────────────────────
            builder.HasData(
                new { Id = DocumentType.QuotationId, Code = "QUOTATION", Name = "Sales Quotation", SortOrder = 1, IsActive = true, CreatedAt = new DateTime(2025, 1, 1) },
                new { Id = DocumentType.DeliveryNoteId, Code = "DELIVERY_NOTE", Name = "Delivery Note", SortOrder = 2, IsActive = true, CreatedAt = new DateTime(2025, 1, 1) },
                new { Id = DocumentType.TaxInvoiceId, Code = "TAX_INVOICE", Name = "Tax Invoice", SortOrder = 3, IsActive = true, CreatedAt = new DateTime(2025, 1, 1) }
            );
        }
    }
}
