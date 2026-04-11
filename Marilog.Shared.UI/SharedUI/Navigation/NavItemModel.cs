using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Shared.UI.SharedUI.Navigation
{
    public class NavItemModel
    {
        public string Key { get; set; } = Guid.NewGuid().ToString();

        public string Title { get; set; } = string.Empty;

        public string? Icon { get; set; }

        public string? Route { get; set; }

        public List<NavItemModel> Children { get; set; } = new();

        public bool HasChildren =>
            Children is { Count: > 0 };
    }
}
