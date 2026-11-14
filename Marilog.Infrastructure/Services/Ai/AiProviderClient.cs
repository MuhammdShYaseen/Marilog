using Marilog.Application.Interfaces.Ai;
using Marilog.Domain.Entities.AiEntities;
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
            var url = provider.BaseUrl.Replace("{{model}}", Uri.EscapeDataString(provider.ModelIdentifier));

            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };

            if (!string.IsNullOrEmpty(apiKey))
                request.Headers.TryAddWithoutValidation(provider.AuthHeaderName, provider.AuthPrefix + apiKey);

            if (provider.ExtraHeadersJson is not null)
                foreach (var (name, value) in JsonSerializer.Deserialize<Dictionary<string, string>>(provider.ExtraHeadersJson)!)
                    request.Headers.TryAddWithoutValidation(name, value);

            using var response = await _http.SendAsync(request, ct);
            var json = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException(
                    $"AI provider '{provider.Name}' returned {(int)response.StatusCode}: {Truncate(json)}");

            return ReadPath(json, provider.ResponseTextPath);
        }

        //----Privates---------------------------------------------------------

        // "choices.0.message.content" -> the text; a non-text value (e.g. an embedding array) comes back as JSON.
        private static string ReadPath(string json, string path)
        {
            var node = JsonNode.Parse(json);

            foreach (var part in path.Split('.', StringSplitOptions.RemoveEmptyEntries))
            {
                node = node switch
                {
                    JsonArray array when int.TryParse(part, out var i) && i >= 0 && i < array.Count => array[i],
                    JsonObject obj => obj[part],
                    _ => null
                };

                if (node is null)
                    throw new InvalidOperationException($"Response path '{path}' not found at '{part}': {Truncate(json)}");
            }

            return node is JsonValue value && value.TryGetValue<string>(out var text)
                ? text
                : node!.ToJsonString();
        }

        private static string Truncate(string text)
            => text.Length <= MaxErrorLength ? text : text[..MaxErrorLength] + "...";
    }
}