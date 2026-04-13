using Marilog.Contracts.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Shared.UI.Services
{
    public class NavigationStateService
    {
        private List<NavItemResponse>? _items;

        public List<NavItemResponse> Items
        {
            get => _items ?? new();
            set => _items = value;
        }

        public bool IsLoaded { get; set; }

        public event Action? OnChange;

        public async Task LoadItemsAsync(Func<Task<List<NavItemResponse>>> loadFunction)
        {
            if (!IsLoaded)
            {
                try
                {
                    Items = await loadFunction();
                    IsLoaded = true;
                    NotifyStateChanged();
                }
                catch
                {
                    Items = new();
                }
            }
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
