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

            // ── Seed — must include Guid (required by Entity base) ────────────────
            builder.HasData(
                new
                {
                    Id = DocumentType.QuotationId,
                    Guid = new Guid("11111111-0000-0000-0000-000000000001"),
                    Code = "QUOTATION",
                    Name = "Sales Quotation",
                    SortOrder = 1,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = (DateTime?)null
                },
                new
                {
                    Id = DocumentType.DeliveryNoteId,
                    Guid = new Guid("11111111-0000-0000-0000-000000000002"),
                    Code = "DELIVERY_NOTE",
                    Name = "Delivery Note",
                    SortOrder = 2,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = (DateTime?)null
                },
                new
                {
                    Id = DocumentType.TaxInvoiceId,
                    Guid = new Guid("11111111-0000-0000-0000-000000000003"),
                    Code = "TAX_INVOICE",
                    Name = "Tax Invoice",
                    SortOrder = 3,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = (DateTime?)null
                }
            );
        }
    }
}
