using Marilog.Contracts.DTOs.Frontend.AppTheme;
using Marilog.Contracts.DTOs.Responses;

namespace Marilog.Shared.UI.Services
{
    public class AppStateService
    {
        private List<NavItemResponse>? _navItems;
        private AppThemeResponse? _theme;

        public List<NavItemResponse> NavItems => _navItems ?? [];
        public AppThemeResponse? Theme => _theme;
        public bool IsLoaded { get; private set; }

        public event Action? OnChange;

        public async Task InitializeAsync(Func<Task<List<NavItemResponse>>> loadNav, Func<Task<AppThemeResponse?>> loadTheme)
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
                _theme = await loadTheme();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Theme load failed: {ex.Message}");
                _theme = null;
            }

            IsLoaded = true;
            OnChange?.Invoke();
        }
    }
}