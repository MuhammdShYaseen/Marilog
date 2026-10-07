using Marilog.Contracts.DTOs.Frontend.AppTheme;
using Marilog.Contracts.DTOs.Responses;

namespace Marilog.Shared.UI.Services
{
    public class AppStateService
    {
        private List<NavItemResponse>? _navItems;
        private AppThemeResponse? _theme;
        private IReadOnlyList<AppThemeResponse> _themes = [];
        public List<NavItemResponse> NavItems => _navItems ?? [];
        public AppThemeResponse? Theme => _theme;
        public IReadOnlyList<AppThemeResponse> Themes => _themes;
        public bool IsLoaded { get; private set; }

        public event Action? OnChange;

        public async Task InitializeAsync(Func<Task<List<NavItemResponse>>> loadNav, Func<Task<IReadOnlyList<AppThemeResponse>>> loadThemes, int? savedThemeId)
        {
            if (IsLoaded) return;

            try
            {
                _navItems = await loadNav();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Nav load failed: {ex.Message}");
                _navItems = [];
            }

            try
            {
                _themes = await loadThemes();
                _theme = _themes.FirstOrDefault(t => t.Id == savedThemeId)
                      ?? _themes.FirstOrDefault(t => t.IsDefault);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Theme load failed: {ex.Message}");
                _themes = [];
                _theme = null;
            }

            IsLoaded = true;
            OnChange?.Invoke();
        }

        public void SetTheme(int themeId)
        {
            var theme = _themes.FirstOrDefault(t => t.Id == themeId);
            if (theme is null || theme.Id == _theme?.Id) return;

            _theme = theme;
            OnChange?.Invoke();
        }

        /// <summary>Called by the theme admin page after any change so edits apply live.</summary>
        public void ReloadThemes(IReadOnlyList<AppThemeResponse> activeThemes)
        {
            _themes = activeThemes;
            _theme = _themes.FirstOrDefault(t => t.Id == _theme?.Id)
                  ?? _themes.FirstOrDefault(t => t.IsDefault);
            OnChange?.Invoke();
        }
    }
}