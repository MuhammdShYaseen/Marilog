using Marilog.Domain.Entities.SystemEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marilog.Infrastructure.DataAccess.Configurations
{
    public class DocumentTemplateConfiguration : IEntityTypeConfiguration<DocumentTemplate>
    {
        public void Configure(EntityTypeBuilder<DocumentTemplate> builder)
        {
            builder.ToTable("DocumentTemplates");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(t => t.Name)
                .IsUnique();

            builder.Property(t => t.FontFamily)
                .IsRequired()
                .HasMaxLength(100);

            // The whole page layout lives in one JSON column: it is always read and saved as one piece.
            builder.OwnsMany(t => t.Elements, element =>
            {
                element.ToJson();
                element.OwnsMany(e => e.Columns);
            });
        }
    }
}