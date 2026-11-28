using Marilog.Domain.Entities.SystemEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marilog.Ship.Data.Configurations
{
    public class DocumentItemConfiguration : IEntityTypeConfiguration<DocumentItem>
    {
        public void Configure(EntityTypeBuilder<DocumentItem> builder)
        {
            builder.ToTable("document_items");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.ProductName).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Unit).HasMaxLength(30);

            builder.HasIndex(x => x.DocumentId);
        }
    }
}