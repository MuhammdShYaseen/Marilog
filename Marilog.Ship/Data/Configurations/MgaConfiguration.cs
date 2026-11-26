using Marilog.Ship.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marilog.Ship.Data.Configurations
{
    public class MgaConfiguration : IEntityTypeConfiguration<Mga>
    {
        public void Configure(EntityTypeBuilder<Mga> builder)
        {
            builder.ToTable("Mgas");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Status)
                   .IsRequired()
                   .HasConversion<string>()
                   .HasMaxLength(20);
            builder.Property(x => x.EndReason).HasMaxLength(300);
            builder.Property(x => x.MasterName).HasMaxLength(150);

            // ── Owned children ────────────────────────────────────────────────────
            builder.HasMany(x => x.CrewLines)
                   .WithOne()
                   .HasForeignKey(x => x.MgaId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.CashLines)
                   .WithOne()
                   .HasForeignKey(x => x.MgaId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.Number).IsUnique();
            builder.HasIndex(x => new { x.FromDate, x.ToDate });
        }
    }
}