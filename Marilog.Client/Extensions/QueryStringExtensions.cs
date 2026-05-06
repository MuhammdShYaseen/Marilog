

namespace Marilog.Client.Extensions
{
    public static class QueryStringExtensions
    {
        public static string ToQueryString(this object filter)
        {
            var props = filter.GetType()
                .GetProperties()
                .Where(p => p.GetValue(filter) != null)
                .Select(p => $"{p.Name}={Uri.EscapeDataString(p.GetValue(filter)!.ToString()!)}");

            var qs = string.Join("&", props);
            return qs.Length > 0 ? $"?{qs}" : string.Empty;
        }
    }
}
