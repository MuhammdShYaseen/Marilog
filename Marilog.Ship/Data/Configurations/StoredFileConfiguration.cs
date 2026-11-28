using Marilog.Domain.Entities.SystemEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marilog.Ship.Data.Configurations
{
    public class StoredFileConfiguration : IEntityTypeConfiguration<StoredFile>
    {
        public void Configure(EntityTypeBuilder<StoredFile> builder)
        {
            builder.ToTable("StoredFiles");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.EntityType).IsRequired().HasConversion<int>();
            builder.Property(x => x.OriginalFileName).IsRequired().HasMaxLength(255);
            builder.Property(x => x.StoredFileName).IsRequired().HasMaxLength(255);
            builder.Property(x => x.RelativePath).IsRequired().HasMaxLength(500);
            builder.Property(x => x.ThumbnailRelativePath).HasMaxLength(500);
            builder.Property(x => x.ContentType).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Checksum).IsRequired().HasMaxLength(128);

            // A folder with files in it can't be deleted.
            builder.HasOne<StoredFolder>()
                   .WithMany()
                   .HasForeignKey(x => x.FolderId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Tags)
                   .WithOne()
                   .HasForeignKey(t => t.StoredFileId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.EntityType, x.EntityId });
            builder.HasIndex(x => x.Checksum);
        }
    }
}