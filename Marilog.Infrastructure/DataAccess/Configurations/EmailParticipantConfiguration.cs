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
            builder.Property(x => x.EmailId).IsRequired();

            builder.Property(x => x.Role)
                   .IsRequired()
                   .HasConversion<string>()
                   .HasMaxLength(10);   // "From" | "To" | "Cc"

            builder.Property(x => x.ParticipantType)
                   .IsRequired()
                   .HasConversion<string>()
                   .HasMaxLength(20);   // "Company" | "Vessel"

            builder.Property(x => x.ParticipantId).IsRequired();
            builder.Property(x => x.DisplayName).HasMaxLength(200);
            builder.Property(x => x.EmailAddress).HasMaxLength(200);

            // ── Conditional navigation: Company ───────────────────────────────────
            builder.HasOne(x => x.Company)
                   .WithMany()
                   .HasForeignKey(x => x.ParticipantId)
                   .HasPrincipalKey(x => x.CompanyID)
                   .IsRequired(false)
                   .OnDelete(DeleteBehavior.NoAction);

            // ── Conditional navigation: Vessel ────────────────────────────────────
            builder.HasOne(x => x.Vessel)
                   .WithMany()
                   .HasForeignKey(x => x.ParticipantId)
                   .HasPrincipalKey(x => x.VesselID)
                   .IsRequired(false)
                   .OnDelete(DeleteBehavior.NoAction);

            // ── Indexes ───────────────────────────────────────────────────────────
            builder.HasIndex(x => x.EmailId);
            builder.HasIndex(x => new { x.ParticipantType, x.ParticipantId });  // "all emails for Company X"
            builder.HasIndex(x => x.Role);
        }
    }
}
