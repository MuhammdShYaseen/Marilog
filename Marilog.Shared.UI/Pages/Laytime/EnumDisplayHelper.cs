// Renders any enum's members as human-readable dropdown options purely via
// reflection, WITHOUT hardcoding member names. This means dropdowns for
// SofEventType / LaytimeExceptionType (whose exact member names weren't
// confirmed) still bind and validate correctly against whatever the real
// enum actually contains, with zero risk of a name mismatch.
using System.Text;

namespace Marilog.Shared.UI.Pages.Laytime
{
    public static class EnumDisplayHelper
    {
        public static IReadOnlyList<(T Value, string Label)> GetOptions<T>() where T : struct, Enum
        {
            return Enum.GetValues<T>()
                .Select(v => (v, ToDisplayLabel(v.ToString())))
                .ToList();
        }

        public static string ToDisplayLabel(string enumName)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < enumName.Length; i++)
            {
                if (i > 0 && char.IsUpper(enumName[i]) && !char.IsUpper(enumName[i - 1]))
                    sb.Append(' ');
                sb.Append(enumName[i]);
            }
            return sb.ToString();
        }
    }
}
