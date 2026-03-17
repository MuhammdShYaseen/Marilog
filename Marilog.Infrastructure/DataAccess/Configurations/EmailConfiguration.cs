using Marilog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marilog.Infrastructure.DataAccess.Configurations
{
    public class EmailConfiguration : IEntityTypeConfiguration<Email>
    {
        public void Configure(EntityTypeBuilder<Email> builder)
        {
            builder.ToTable("Emails");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).UseIdentityColumn();

            builder.Property(x => x.EntityType).IsRequired().HasMaxLength(50);
            builder.Property(x => x.EntityId).IsRequired();
            builder.Property(x => x.Subject).IsRequired().HasMaxLength(300);
            builder.Property(x => x.Body).IsRequired().HasColumnType("nvarchar(max)");

            builder.Property(x => x.Direction)
                   .IsRequired()
                   .HasConversion<string>()
                   .HasMaxLength(20);

            builder.Property(x => x.Status)
                   .IsRequired()
                   .HasConversion<string>()
                   .HasMaxLength(20)
                   .HasDefaultValue(EmailStatus.Draft);

            builder.Property(x => x.SentAt);
            builder.Property(x => x.ExternalId).HasMaxLength(200);
            builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
            builder.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

            // ── Participants ──────────────────────────────────────────────────────
            builder.HasMany(x => x.Participants)
                   .WithOne()
                   .HasForeignKey(nameof(EmailParticipant.EmailId))
                   .IsRequired(false)
                   .OnDelete(DeleteBehavior.Cascade);

            // ── Attachments ───────────────────────────────────────────────────────
            builder.HasMany(x => x.Attachments)
                   .WithOne()
                   .HasForeignKey(nameof(EmailAttachment.EmailId))
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.EntityType, x.EntityId });
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.SentAt);
            builder.HasIndex(x => x.ExternalId);
        }
    }
}
