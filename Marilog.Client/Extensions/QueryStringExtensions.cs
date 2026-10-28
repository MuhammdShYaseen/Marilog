

using System.Globalization;

namespace Marilog.Client.Extensions
{
    public static class QueryStringExtensions
    {
        public static string ToQueryString(this object filter)
        {
            var props = filter.GetType()
                .GetProperties()
                .Where(p => p.GetValue(filter) != null)
                .Select(p => $"{p.Name}={Uri.EscapeDataString(FormatValue(p.GetValue(filter)!))}");

            var qs = string.Join("&", props);
            return qs.Length > 0 ? $"?{qs}" : string.Empty;
        }
        private static string FormatValue(object value) => value switch
        {
            DateOnly d => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            DateTime dt => dt.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture),
            IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString()!
        };
    }
}
