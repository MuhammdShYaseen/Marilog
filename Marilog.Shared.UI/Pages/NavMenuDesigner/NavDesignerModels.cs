using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Shared.UI.Pages.NavMenuDesigner
{
    public record NavPageOption(string Route, string Title)
    {
        public static string Normalize(string route) => route.Trim().Trim('/').ToLowerInvariant();
    }

    public record NavParentOption(int Id, string Path);

    public class NavItemFormModel
    {
        public string Title { get; set; } = "";
        public string? Route { get; set; }
        public string? Icon { get; set; }
        public int? ParentId { get; set; }
    }
}
