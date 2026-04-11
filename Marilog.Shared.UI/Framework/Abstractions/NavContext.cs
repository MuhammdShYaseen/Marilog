

namespace Marilog.Shared.UI.Framework.Abstractions
{
    public sealed class NavContext
    {
        // =========================
        // ACTIVE STATE
        // =========================

        private string? _activeKey;

        /// <summary>
        /// Currently active navigation key (NavItem Id/Key).
        /// </summary>
        public string? ActiveKey
        {
            get => _activeKey;
            private set
            {
                if (_activeKey == value)
                    return;

                _activeKey = value;
                NotifyStateChanged();
            }
        }

        // =========================
        // EVENTS
        // =========================

        public event Action? OnChange;

        private void NotifyStateChanged()
        {
            OnChange?.Invoke();
        }

        // =========================
        // API
        // =========================

        /// <summary>
        /// Sets the active navigation item.
        /// </summary>
        public void SetActive(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;

            ActiveKey = key;
        }

        /// <summary>
        /// Clears current selection.
        /// </summary>
        public void Clear()
        {
            ActiveKey = null;
        }

        /// <summary>
        /// Checks if a key is active.
        /// </summary>
        public bool IsActive(string? key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;

            return string.Equals(_activeKey, key, StringComparison.Ordinal);
        }
    }
}
