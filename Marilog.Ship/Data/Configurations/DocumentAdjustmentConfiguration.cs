using Marilog.Domain.Entities.SystemEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marilog.Ship.Data.Configurations
{
    // The Document → Adjustments relationship is set in DocumentConfiguration.
    public class DocumentAdjustmentConfiguration : IEntityTypeConfiguration<DocumentAdjustment>
    {
        public void Configure(EntityTypeBuilder<DocumentAdjustment> builder)
        {
            builder.ToTable("DocumentAdjustments");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Reason).HasMaxLength(500);

            builder.HasIndex(x => x.DocumentId);
        }
    }
}