using Marilog.Domain.Entities.Frontend;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marilog.Infrastructure.DataAccess.Configurations
{
    public class AppThemeConfiguration : IEntityTypeConfiguration<AppTheme>
    {
        public void Configure(EntityTypeBuilder<AppTheme> builder)
        {
            builder.ToTable("AppThemes");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.ThemeName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(t => t.ThemeKey)
                .IsRequired()
                .HasMaxLength(20);

            builder.HasIndex(t => t.ThemeKey)
                .IsUnique();

            builder.Property(t => t.IsDefault)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(t => t.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // ── Colors ────────────────────────────────────────────────────────────

            builder.Property(t => t.PrimaryColor)
                .IsRequired()
                .HasMaxLength(7);

            builder.Property(t => t.SecondaryColor)
                .IsRequired()
                .HasMaxLength(7);

            builder.Property(t => t.AppBarColor)
                .IsRequired()
                .HasMaxLength(7);

            builder.Property(t => t.BackgroundColor)
                .IsRequired()
                .HasMaxLength(7);

            builder.Property(t => t.SurfaceColor)
                .IsRequired()
                .HasMaxLength(7);

            builder.Property(t => t.ErrorColor)
                .IsRequired()
                .HasMaxLength(7);

            builder.Property(t => t.SuccessColor)
                .IsRequired()
                .HasMaxLength(7);

            builder.Property(t => t.WarningColor)
                .IsRequired()
                .HasMaxLength(7);

            // ── Typography ────────────────────────────────────────────────────────

            builder.Property(t => t.FontFamily)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(t => t.BaseFontSize)
                .IsRequired()
                .HasMaxLength(10);
        }
    }
}