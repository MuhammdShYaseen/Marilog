using Marilog.Domain.Entities.SystemEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marilog.Infrastructure.DataAccess.Configurations
{
    public class StoredFileConfiguration : IEntityTypeConfiguration<StoredFile>
    {
        public void Configure(EntityTypeBuilder<StoredFile> builder)
        {
            builder.ToTable("StoredFiles");

            // ── Key ─────────────────────────────────────────────
            builder.HasKey(x => x.Id);

            // ── Entity Polymorphic Link ─────────────────────────
            builder.Property(x => x.EntityType)
                .HasConversion<int>() // Enum stored as int
                .IsRequired(true);

            builder.Property(x => x.EntityId)
                .IsRequired(false);

            // Composite index for fast lookup
            builder.HasIndex(x => new { x.EntityType, x.EntityId });

            // ── File Identity ───────────────────────────────────
            builder.Property(x => x.OriginalFileName)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.StoredFileName)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.RelativePath)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(x => x.ContentType)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Size)
                .IsRequired();


            builder.HasMany(d => d.Tags)
                   .WithOne()
                   .HasForeignKey(t => t.StoredFileId)
                   .OnDelete(DeleteBehavior.Cascade);


            // ── OCR Content (optional large text) ───────────────
            builder.Property(x => x.Content)
                .HasColumnType("nvarchar(max)");

            // ── Checksum ────────────────────────────────────────
            builder.Property(x => x.Checksum)
                .HasMaxLength(128)
                .IsRequired();

            // ── Optional performance index (deduplication use) ──
            builder.HasIndex(x => x.Checksum);
        }
    }
}
