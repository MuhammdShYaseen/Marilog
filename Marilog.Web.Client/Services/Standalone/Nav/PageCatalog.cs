using Marilog.Shared.UI.Pages.NavMenuDesigner;
using Microsoft.AspNetCore.Components;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Marilog.Web.Client.Services.Standalone.Nav   // adjust to your folder
{
    public static class PageCatalog
    {
        private static List<NavPageOption>? _pages;

        public static IReadOnlyList<NavPageOption> Pages => _pages ??= Discover();

        private static List<NavPageOption> Discover() =>
            AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.GetName().Name?.StartsWith("Marilog", StringComparison.OrdinalIgnoreCase) == true)
                .SelectMany(SafeGetTypes)
                .Where(t => !t.IsAbstract && typeof(IComponent).IsAssignableFrom(t))
                .SelectMany(t => t.GetCustomAttributes<Microsoft.AspNetCore.Components.RouteAttribute>()
                                  .Select(r => new { Type = t, r.Template }))
                .Where(x => !x.Template.Contains('{'))          // routes with parameters can't be menu links
                .GroupBy(x => NavPageOption.Normalize(x.Template))
                .Select(g => g.First())
                .Select(x => new NavPageOption(x.Template, ToTitle(x.Type.Name)))
                .OrderBy(p => p.Title)
                .ToList();

        private static IEnumerable<Type> SafeGetTypes(Assembly assembly)
        {
            try { return assembly.GetTypes(); }
            catch (ReflectionTypeLoadException ex) { return ex.Types.Where(t => t is not null)!; }
        }

        // "VesselCertificatesPage" -> "Vessel Certificates"
        private static string ToTitle(string typeName)
        {
            var name = typeName.EndsWith("Page") && typeName.Length > 4 ? typeName[..^4] : typeName;
            return Regex.Replace(name, "(?<=[a-z])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])", " ");
        }
    }
}