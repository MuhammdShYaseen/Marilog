using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Marilog.Client.Extensions
{
    public static class HttpResponseExtensions
    {
        public static async Task EnsureSuccessWithMessageAsync(this HttpResponseMessage response, CancellationToken ct = default)
        {
            if (response.IsSuccessStatusCode) return;

            var body = await response.Content.ReadAsStringAsync(ct);
            var message = ExtractMessage(body) ?? response.StatusCode switch
            {
                HttpStatusCode.Forbidden => "You don't have permission to do this.",
                HttpStatusCode.Unauthorized => "Your session has expired. Please sign in again.",
                _ => $"Request failed ({(int)response.StatusCode} {response.ReasonPhrase})."
            };

            throw new HttpRequestException(message, null, response.StatusCode);
        }

        public static async Task<T?> GetCheckedAsync<T>(this HttpClient http, string url, CancellationToken ct = default)
        {
            using var response = await http.GetAsync(url, ct);
            await response.EnsureSuccessWithMessageAsync(ct);
            return await response.Content.ReadFromJsonAsync<T>(ct);
        }

        private static string? ExtractMessage(string body)
        {
            if (string.IsNullOrWhiteSpace(body)) return null;

            try
            {
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;
                if (root.ValueKind != JsonValueKind.Object) return null;

                var message = GetString(root, "message") ?? GetString(root, "detail") ?? GetString(root, "title");
                var errors = GetErrors(root);

                if (errors.Count == 0) return message;
                var joined = string.Join("; ", errors);
                return message is null ? joined : $"{message} — {joined}";
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static string? GetString(JsonElement root, string name)
        {
            foreach (var p in root.EnumerateObject())
                if (p.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && p.Value.ValueKind == JsonValueKind.String)
                    return p.Value.GetString();
            return null;
        }

        private static List<string> GetErrors(JsonElement root)
        {
            var result = new List<string>();

            foreach (var p in root.EnumerateObject())
            {
                if (!p.Name.Equals("errors", StringComparison.OrdinalIgnoreCase)) continue;

                if (p.Value.ValueKind == JsonValueKind.Array)
                    result.AddRange(StringsOf(p.Value));
                else if (p.Value.ValueKind == JsonValueKind.Object)
                    foreach (var field in p.Value.EnumerateObject())
                        if (field.Value.ValueKind == JsonValueKind.Array)
                            result.AddRange(StringsOf(field.Value));
            }

            return result;
        }

        private static IEnumerable<string> StringsOf(JsonElement array)
            => array.EnumerateArray()
                    .Where(e => e.ValueKind == JsonValueKind.String)
                    .Select(e => e.GetString()!);
    }
}