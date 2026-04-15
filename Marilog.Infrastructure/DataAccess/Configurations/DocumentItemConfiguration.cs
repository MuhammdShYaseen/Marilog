using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Marilog.Domain.Entities.SystemEntities;

namespace Marilog.Infrastructure.DataAccess.Configurations
{
    public class DocumentItemConfiguration : IEntityTypeConfiguration<DocumentItem>
    {
        public void Configure(EntityTypeBuilder<DocumentItem> builder)
        {
            builder.ToTable("document_items");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).UseIdentityColumn();
            builder.Property(x => x.ProductName).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Quantity).IsRequired().HasColumnType("decimal(14,4)");
            builder.Property(x => x.Unit).HasMaxLength(30);
            builder.Property(x => x.UnitPrice).IsRequired().HasColumnType("decimal(14,4)");
            builder.Property(x => x.LineTotal).IsRequired().HasColumnType("decimal(18,2)");

            builder.HasIndex(x => x.DocumentId);
        }
    }
}
