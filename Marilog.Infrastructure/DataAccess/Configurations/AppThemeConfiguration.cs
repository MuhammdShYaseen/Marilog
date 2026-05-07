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

            // ── Seed Data ─────────────────────────────────────────────────────────

            builder.HasData(
                AppTheme.Create(
                    themeName: "Marilog Light",
                    themeKey: "light",
                    isDefault: true,
                    primaryColor: "#1E88E5",
                    secondaryColor: "#42A5F5",
                    appBarColor: "#1565C0",
                    backgroundColor: "#F5F5F5",
                    surfaceColor: "#FFFFFF",
                    errorColor: "#E53935",
                    successColor: "#43A047",
                    warningColor: "#FB8C00",
                    fontFamily: "Roboto",
                    baseFontSize: "14px",
                    isDarkMode: false),

                AppTheme.Create(
                    themeName: "Marilog Dark",
                    themeKey: "dark",
                    isDefault: false,
                    primaryColor: "#90CAF9",
                    secondaryColor: "#42A5F5",
                    appBarColor: "#0D47A1",
                    backgroundColor: "#121212",
                    surfaceColor: "#1E1E1E",
                    errorColor: "#EF9A9A",
                    successColor: "#A5D6A7",
                    warningColor: "#FFE082",
                    fontFamily: "Roboto",
                    baseFontSize: "14px",
                    isDarkMode: true));
        }
    }
}
