using Marilog.Application.Interfaces.Ai;
using Marilog.Domain.Entities.AiEntities;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Marilog.Infrastructure.Services.Ai
{
    internal sealed class AiProviderClient : IAiProviderClient
    {
        private const int MaxErrorLength = 500;
        private readonly HttpClient _http;

        public AiProviderClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<string> SendAsync(AiProvider provider, string? apiKey, string body, CancellationToken ct)
        {
            using var request = CreateRequest(provider, apiKey, body);
            using var response = await _http.SendAsync(request, ct);
            var json = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException(
                    $"AI provider '{provider.Name}' returned {(int)response.StatusCode}: {Truncate(json)}");

            var node = FindPath(JsonNode.Parse(json), provider.ResponseTextPath)
                ?? throw new InvalidOperationException(
                    $"Response path '{provider.ResponseTextPath}' not found: {Truncate(json)}");

            return AsText(node);
        }

        // Reads SSE ("data: {...}") or NDJSON (Ollama) line by line and yields the text at StreamTextPath.
        // Lines without that path (start/stop events, pings, "[DONE]") are skipped.
        public async IAsyncEnumerable<string> StreamAsync(
            AiProvider provider, string? apiKey, string body, [EnumeratorCancellation] CancellationToken ct)
        {
            using var request = CreateRequest(provider, apiKey, body);
            using var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException(
                    $"AI provider '{provider.Name}' returned {(int)response.StatusCode}: {Truncate(await response.Content.ReadAsStringAsync(ct))}");

            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync(ct));

            while (await reader.ReadLineAsync(ct) is { } line)
            {
                var data = line.StartsWith("data:", StringComparison.Ordinal) ? line[5..].Trim() : line.Trim();
                if (data.Length == 0 || data[0] != '{')
                    continue;

                var chunk = JsonNode.Parse(data);
                if (chunk?["error"] is not null)
                    throw new InvalidOperationException($"AI provider '{provider.Name}' stream error: {Truncate(data)}");

                if (FindPath(chunk, provider.StreamTextPath!) is { } node && AsText(node) is { Length: > 0 } text)
                    yield return text;
            }
        }

        //----Privates---------------------------------------------------------

        private static HttpRequestMessage CreateRequest(AiProvider provider, string? apiKey, string body)
        {
            var url = provider.BaseUrl.Replace("{{model}}", Uri.EscapeDataString(provider.ModelIdentifier));

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };

            if (!string.IsNullOrEmpty(apiKey))
                request.Headers.TryAddWithoutValidation(provider.AuthHeaderName, provider.AuthPrefix + apiKey);

            if (provider.ExtraHeadersJson is not null)
                foreach (var (name, value) in JsonSerializer.Deserialize<Dictionary<string, string>>(provider.ExtraHeadersJson)!)
                    request.Headers.TryAddWithoutValidation(name, value);

            return request;
        }

        // "choices.0.message.content" -> the node, or null when the path is missing.
        private static JsonNode? FindPath(JsonNode? node, string path)
        {
            foreach (var part in path.Split('.', StringSplitOptions.RemoveEmptyEntries))
            {
                node = node switch
                {
                    JsonArray array when int.TryParse(part, out var i) && i >= 0 && i < array.Count => array[i],
                    JsonObject obj => obj[part],
                    _ => null
                };

                if (node is null) return null;
            }

            return node;
        }

        // Text stays text; anything else (e.g. an embedding array) comes back as JSON.
        private static string AsText(JsonNode node)
            => node is JsonValue value && value.TryGetValue<string>(out var text) ? text : node.ToJsonString();

        private static string Truncate(string text)
            => text.Length <= MaxErrorLength ? text : text[..MaxErrorLength] + "...";
    }
}