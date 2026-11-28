using Marilog.Domain.Entities.SystemEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marilog.Ship.Data.Configurations
{
    public class StoredFolderConfiguration : IEntityTypeConfiguration<StoredFolder>
    {
        public void Configure(EntityTypeBuilder<StoredFolder> builder)
        {
            builder.ToTable("StoredFolders");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(StoredFolder.NameMaxLength);
            builder.Property(x => x.EntityType).IsRequired().HasConversion<int>();

            builder.HasOne<StoredFolder>()
                   .WithMany()
                   .HasForeignKey(x => x.ParentFolderId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.EntityType, x.EntityId, x.ParentFolderId, x.Name }).IsUnique();
        }
    }
}