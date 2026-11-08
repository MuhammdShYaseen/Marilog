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
            // ── Indexes ───────────────────────────────────────────────────

            builder.HasIndex(x => new { x.EntityType, x.EntityId, x.CreatedAt });

            builder.HasIndex(x => new { x.UserId, x.CreatedAt });
        }
    }
}