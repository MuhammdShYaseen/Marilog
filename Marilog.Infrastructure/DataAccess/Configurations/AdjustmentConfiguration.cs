using Marilog.Domain.Entities.SystemEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marilog.Infrastructure.DataAccess.Configurations
{
    public class AdjustmentConfiguration : IEntityTypeConfiguration<DocumentAdjustment>
    {
        public void Configure(EntityTypeBuilder<DocumentAdjustment> builder)
        {
            builder.ToTable("DocumentAdjustments");

            builder.HasKey(a => a.Id);
            builder.Property(x => x.Id).UseIdentityColumn();
            builder.Property(a => a.DocumentId)
                   .IsRequired();

            builder.Property(a => a.Amount)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(a => a.AdjustmentDate)
                   .HasColumnType("date")
                   .IsRequired();

            builder.Property(a => a.Reason)
                   .HasMaxLength(500);

            builder.HasOne<Document>()
                   .WithMany(d => d.Adjustments)
                   .HasForeignKey(a => a.DocumentId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(a => a.DocumentId);

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_DocumentAdjustments_Amount_NotZero", "[Amount] <> 0"));
        }
    }
}
