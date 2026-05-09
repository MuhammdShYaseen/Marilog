using Marilog.Contracts.DTOs.Frontend.AppTheme;
using Marilog.Contracts.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Text;

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

        public async Task InitializeAsync(
            Func<Task<List<NavItemResponse>>> loadNav,
            Func<Task<AppThemeResponse?>> loadTheme)
        {
            if (IsLoaded) return;

            try
            {
                _navItems = await loadNav();
                _theme = await loadTheme();
                IsLoaded = true;
                OnChange?.Invoke();
            }
            catch
            {
                _navItems = [];
            }
        }
    }
}