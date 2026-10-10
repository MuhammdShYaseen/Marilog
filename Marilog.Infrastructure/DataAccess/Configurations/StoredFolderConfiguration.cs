using Marilog.Domain.Entities.SystemEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marilog.Infrastructure.DataAccess.Configurations
{
    public class StoredFolderConfiguration : IEntityTypeConfiguration<StoredFolder>
    {
        public void Configure(EntityTypeBuilder<StoredFolder> builder)
        {
            builder.ToTable("StoredFolders");

            // ── Key ─────────────────────────────────────────────
            builder.HasKey(x => x.Id);

            // ── Name ────────────────────────────────────────────
            builder.Property(x => x.Name)
                .HasMaxLength(StoredFolder.NameMaxLength)
                .IsRequired();

            // ── Entity Polymorphic Link (same as StoredFile) ────
            builder.Property(x => x.EntityType)
                .HasConversion<int>()
                .IsRequired(true);

            builder.Property(x => x.EntityId)
                .IsRequired(false);

            // ── Parent (self-reference) ─────────────────────────
            builder.Property(x => x.ParentFolderId)
                .IsRequired(false);

            builder.HasOne<StoredFolder>()
                .WithMany()
                .HasForeignKey(x => x.ParentFolderId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── No duplicate names in the same place ────────────
            builder.HasIndex(x => new { x.EntityType, x.EntityId, x.ParentFolderId, x.Name })
                .IsUnique()
                .HasFilter(null);
        }
    }
}