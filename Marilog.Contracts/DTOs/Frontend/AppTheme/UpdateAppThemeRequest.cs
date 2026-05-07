using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Contracts.DTOs.Frontend.AppTheme
{
    public class UpdateAppThemeRequest
    {
        public string? ThemeName { get; set; }
        public string? PrimaryColor { get; set; }
        public string? SecondaryColor { get; set; }
        public string? AppBarColor { get; set; }
        public string? BackgroundColor { get; set; }
        public string? SurfaceColor { get; set; }
        public string? ErrorColor { get; set; }
        public string? SuccessColor { get; set; }
        public string? WarningColor { get; set; }
        public string? FontFamily { get; set; }
        public string? BaseFontSize { get; set; }
        public bool? IsDarkMode { get; set; }
        public bool? IsDefault { get; set; }
    }
}
