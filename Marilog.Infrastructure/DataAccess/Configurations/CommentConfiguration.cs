using Marilog.Domain.Entities.SystemEntities;
using Marilog.Infrastructure.Models.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marilog.Infrastructure.DataAccess.Configurations
{
    public class CommentConfiguration : IEntityTypeConfiguration<Comment>
    {
        public void Configure(EntityTypeBuilder<Comment> builder)
        {
            builder.ToTable("Comments");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).UseIdentityColumn();

            builder.Property(x => x.Text)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.UserId)
                .IsRequired();

            builder.Property(x => x.EntityType)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            builder.Property(x => x.UserId)
                   .IsRequired();

            builder.Property(x => x.UserName)
                   .IsRequired()
                   .HasMaxLength(256);

            builder.OwnsMany(c => c.Reads, b =>
            {
                b.WithOwner().HasForeignKey("CommentId");
                b.ToTable("CommentReads");
                b.HasKey("CommentId", nameof(CommentRead.UserId));
                b.Property(r => r.ReadAt).IsRequired();
            });

            builder.Navigation(c => c.Reads)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            // ── Indexes ───────────────────────────────────────────────────

            builder.HasIndex(x => new { x.EntityType, x.EntityId, x.CreatedAt });

            builder.HasIndex(x => new { x.UserId, x.CreatedAt });
        }
    }
}