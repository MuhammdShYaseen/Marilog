using Marilog.Domain.Common;
using System.Text.RegularExpressions;

namespace Marilog.Domain.Entities.Frontend
{
    public class AppTheme : Entity
    {
        public string ThemeName { get; private set; } = default!;
        public string ThemeKey { get; private set; } = default!;
        public bool IsDefault { get; private set; }
        public string PrimaryColor { get; private set; } = default!;
        public string SecondaryColor { get; private set; } = default!;
        public string AppBarColor { get; private set; } = default!;
        public string BackgroundColor { get; private set; } = default!;
        public string SurfaceColor { get; private set; } = default!;
        public string ErrorColor { get; private set; } = default!;
        public string SuccessColor { get; private set; } = default!;
        public string WarningColor { get; private set; } = default!;
        public string FontFamily { get; private set; } = default!;
        public string BaseFontSize { get; private set; } = default!;
        public bool IsDarkMode { get; private set; }

        private AppTheme() { }

        private AppTheme(
            string themeName,
            string themeKey,
            bool isDefault,
            string primaryColor,
            string secondaryColor,
            string appBarColor,
            string backgroundColor,
            string surfaceColor,
            string errorColor,
            string successColor,
            string warningColor,
            string fontFamily,
            string baseFontSize,
            bool isDarkMode)
        {
            ThemeName = themeName;
            ThemeKey = themeKey;
            IsDefault = isDefault;
            PrimaryColor = primaryColor;
            SecondaryColor = secondaryColor;
            AppBarColor = appBarColor;
            BackgroundColor = backgroundColor;
            SurfaceColor = surfaceColor;
            ErrorColor = errorColor;
            SuccessColor = successColor;
            WarningColor = warningColor;
            FontFamily = fontFamily;
            BaseFontSize = baseFontSize;
            IsDarkMode = isDarkMode;

            Validate();
        }

        public static AppTheme Create(
            string themeName,
            string themeKey,
            bool isDefault,
            string primaryColor,
            string secondaryColor,
            string appBarColor,
            string backgroundColor,
            string surfaceColor,
            string errorColor,
            string successColor,
            string warningColor,
            string fontFamily,
            string baseFontSize,
            bool isDarkMode)
        {
            return new AppTheme(
                themeName, themeKey, isDefault,
                primaryColor, secondaryColor, appBarColor,
                backgroundColor, surfaceColor, errorColor,
                successColor, warningColor,
                fontFamily, baseFontSize, isDarkMode);
        }

        public void Update(
            string? themeName = null,
            string? primaryColor = null,
            string? secondaryColor = null,
            string? appBarColor = null,
            string? backgroundColor = null,
            string? surfaceColor = null,
            string? errorColor = null,
            string? successColor = null,
            string? warningColor = null,
            string? fontFamily = null,
            string? baseFontSize = null,
            bool? isDarkMode = null,
            bool? isDefault = null)
        {
            ThemeName = themeName ?? ThemeName;
            PrimaryColor = primaryColor ?? PrimaryColor;
            SecondaryColor = secondaryColor ?? SecondaryColor;
            AppBarColor = appBarColor ?? AppBarColor;
            BackgroundColor = backgroundColor ?? BackgroundColor;
            SurfaceColor = surfaceColor ?? SurfaceColor;
            ErrorColor = errorColor ?? ErrorColor;
            SuccessColor = successColor ?? SuccessColor;
            WarningColor = warningColor ?? WarningColor;
            FontFamily = fontFamily ?? FontFamily;
            BaseFontSize = baseFontSize ?? BaseFontSize;
            IsDarkMode = isDarkMode ?? IsDarkMode;

            if (isDefault.HasValue)
                IsDefault = isDefault.Value;

            Validate();
        }

        public void SetAsDefault() => IsDefault = true;
        public void UnsetDefault() => IsDefault = false;

        private static readonly Regex HexColorRegex =
    new("^#(?:[0-9A-Fa-f]{3}|[0-9A-Fa-f]{6})$", RegexOptions.Compiled);

        private static readonly Regex FontSizeRegex =
            new(@"^\d+(\.\d+)?(px|rem|em)$", RegexOptions.Compiled);

        private void Validate()
        {
            if (string.IsNullOrWhiteSpace(ThemeName))
                throw new ArgumentException("Theme name is required");

            if (ThemeName.Length > 100)
                throw new ArgumentException("Theme name must not exceed 100 characters");

            if (string.IsNullOrWhiteSpace(ThemeKey))
                throw new ArgumentException("Theme key is required");

            if (ThemeKey.Length > 20)
                throw new ArgumentException("Theme key must not exceed 20 characters");

            ValidateColor(PrimaryColor, nameof(PrimaryColor));
            ValidateColor(SecondaryColor, nameof(SecondaryColor));
            ValidateColor(AppBarColor, nameof(AppBarColor));
            ValidateColor(BackgroundColor, nameof(BackgroundColor));
            ValidateColor(SurfaceColor, nameof(SurfaceColor));
            ValidateColor(ErrorColor, nameof(ErrorColor));
            ValidateColor(SuccessColor, nameof(SuccessColor));
            ValidateColor(WarningColor, nameof(WarningColor));

            if (string.IsNullOrWhiteSpace(FontFamily))
                throw new ArgumentException("Font family is required");

            if (FontFamily.Length > 100)
                throw new ArgumentException("Font family must not exceed 100 characters");

            if (string.IsNullOrWhiteSpace(BaseFontSize) || !FontSizeRegex.IsMatch(BaseFontSize))
                throw new ArgumentException("Base font size must be like 14px, 0.875rem or 1em");
        }

        private static void ValidateColor(string color, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(color) || !IsValidHexColor(color))
                throw new ArgumentException($"{fieldName} must be a valid hex color (e.g. #RRGGBB)");
        }

        private static bool IsValidHexColor(string color) => HexColorRegex.IsMatch(color);
    }
}
