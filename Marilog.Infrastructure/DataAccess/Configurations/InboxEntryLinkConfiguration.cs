using Marilog.Domain.Entities.SystemEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marilog.Infrastructure.DataAccess.Configurations
{
    public class InboxEntryLinkConfiguration : IEntityTypeConfiguration<InboxEntryLink>
    {
        public void Configure(EntityTypeBuilder<InboxEntryLink> builder)
        {
            builder.ToTable("InboxEntryLinks");

            builder.HasKey(x => x.Id);

            builder.HasOne<InboxEntry>()
                .WithMany()
                .HasForeignKey(x => x.InboxEntryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.InboxEntryId, x.EntityType, x.EntityId }).IsUnique();
            builder.HasIndex(x => new { x.EntityType, x.EntityId });
        }
    }
}
