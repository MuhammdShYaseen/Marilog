

namespace Marilog.Contracts.DTOs.Frontend.AppTheme
{
    public class AppThemeResponse
    {
        public int Id { get; set; }
        public string ThemeName { get; set; } = default!;
        public string ThemeKey { get; set; } = default!;
        public bool IsDefault { get; set; }
        public string PrimaryColor { get; set; } = default!;
        public string SecondaryColor { get; set; } = default!;
        public string AppBarColor { get; set; } = default!;
        public string BackgroundColor { get; set; } = default!;
        public string SurfaceColor { get; set; } = default!;
        public string ErrorColor { get; set; } = default!;
        public string SuccessColor { get; set; } = default!;
        public string WarningColor { get; set; } = default!;
        public string FontFamily { get; set; } = default!;
        public string BaseFontSize { get; set; } = default!;
        public bool IsDarkMode { get; set; }
    }
}
