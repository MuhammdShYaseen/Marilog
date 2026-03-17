using Marilog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marilog.Infrastructure.DataAccess.Configurations
{
    public class EmailParticipantConfiguration : IEntityTypeConfiguration<EmailParticipant>
    {
        public void Configure(EntityTypeBuilder<EmailParticipant> builder)
        {
            builder.ToTable("EmailParticipants");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).UseIdentityColumn();

            // ── EmailId — owned by EmailConfiguration.HasMany, not declared here ──
            // Declaring it here causes EF to create a duplicate EmailId1 shadow FK.

            builder.Property(x => x.Role)
                   .IsRequired()
                   .HasConversion<string>()
                   .HasMaxLength(10);

            builder.Property(x => x.ParticipantType)
                   .IsRequired()
                   .HasConversion<string>()
                   .HasMaxLength(20);

            builder.Property(x => x.ParticipantId).IsRequired();
            builder.Property(x => x.DisplayName).HasMaxLength(200);
            builder.Property(x => x.EmailAddress).HasMaxLength(200);

            // ── Polymorphic navigations — no real DB FK ───────────────────────────
            builder.Ignore(x => x.Company);
            //builder.Ignore(x => x.Vessel);

            // ── Indexes ───────────────────────────────────────────────────────────
            // EmailId index is created automatically by EF via the FK in EmailConfiguration
            builder.HasIndex(x => new { x.ParticipantType, x.ParticipantId });
            builder.HasIndex(x => x.Role);
        }
    }
}