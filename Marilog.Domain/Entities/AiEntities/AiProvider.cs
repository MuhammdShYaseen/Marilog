using Marilog.Domain.Common;
using Marilog.Kernel.Enums;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Marilog.Domain.Entities.AiEntities
{
    // One AI endpoint. The body comes from RequestTemplateJson (GenericAiRequestBuilder),
    // the answer is read from ResponseTextPath (e.g. "choices.0.message.content").
    // One active provider per Purpose (AiProviderService.ActivateAsync).
    public class AiProvider : Entity
    {
        public string Name { get; private set; } = default!;
        public AiProviderType ProviderType { get; private set; }
        public AiProviderPurpose Purpose { get; private set; }

        // Full endpoint URL. May contain {{model}} (Gemini puts the model in the URL).
        public string BaseUrl { get; private set; } = default!;
        public string ModelIdentifier { get; private set; } = default!;

        // OpenAI: "Authorization" + "Bearer ". Anthropic: "x-api-key". Azure: "api-key". Gemini: "x-goog-api-key".
        public string AuthHeaderName { get; private set; } = default!;
        public string AuthPrefix { get; private set; } = string.Empty;

        [AuditIgnore]
        public string? ApiKeyEncrypted { get; private set; }
        public string? ApiKeyHint { get; private set; }

        // JSON object of extra headers, e.g. {"anthropic-version":"2023-06-01"}.
        public string? ExtraHeadersJson { get; private set; }

        public string RequestTemplateJson { get; private set; } = default!;
        public string ResponseTextPath { get; private set; } = default!;

        public int MaxInputTokens { get; private set; }
        public int MaxOutputTokens { get; private set; }
        public int ChunkOverlapTokens { get; private set; }
        public decimal DefaultTemperature { get; private set; }

        private AiProvider() { }

        // Created inactive: test it first, then activate.
        public static AiProvider Create(
            string name,
            AiProviderType providerType,
            AiProviderPurpose purpose,
            string baseUrl,
            string modelIdentifier,
            string authHeaderName,
            string? authPrefix,
            string? extraHeadersJson,
            string requestTemplateJson,
            string responseTextPath,
            int maxInputTokens,
            int maxOutputTokens,
            int chunkOverlapTokens,
            decimal defaultTemperature)
        {
            var provider = new AiProvider { Purpose = purpose, IsActive = false };

            provider.Update(name, providerType, baseUrl, modelIdentifier, authHeaderName, authPrefix,
                extraHeadersJson, requestTemplateJson, responseTextPath,
                maxInputTokens, maxOutputTokens, chunkOverlapTokens, defaultTemperature);

            return provider;
        }

        // Purpose and IsActive are not changed here.
        public void Update(
            string name,
            AiProviderType providerType,
            string baseUrl,
            string modelIdentifier,
            string authHeaderName,
            string? authPrefix,
            string? extraHeadersJson,
            string requestTemplateJson,
            string responseTextPath,
            int maxInputTokens,
            int maxOutputTokens,
            int chunkOverlapTokens,
            decimal defaultTemperature)
        {
            baseUrl = Required(baseUrl, nameof(BaseUrl));
            if (!Uri.TryCreate(baseUrl.Replace("{{model}}", "m"), UriKind.Absolute, out var uri)
                || uri.Scheme is not ("http" or "https"))
                throw new ArgumentException("BaseUrl must be a full http(s) URL.", nameof(BaseUrl));

            requestTemplateJson = Required(requestTemplateJson, nameof(RequestTemplateJson));
            EnsureJsonObject(requestTemplateJson, nameof(RequestTemplateJson));

            extraHeadersJson = string.IsNullOrWhiteSpace(extraHeadersJson) ? null : extraHeadersJson.Trim();
            if (extraHeadersJson is not null)
                EnsureHeaders(extraHeadersJson);

            if (maxInputTokens <= 0 || maxOutputTokens <= 0 || chunkOverlapTokens < 0)
                throw new ArgumentException("Token limits must be positive.", nameof(MaxOutputTokens));

            if (defaultTemperature is < 0 or > 2)
                throw new ArgumentException("Temperature must be between 0 and 2.", nameof(DefaultTemperature));

            Name = Required(name, nameof(Name));
            ProviderType = providerType;
            BaseUrl = baseUrl;
            ModelIdentifier = Required(modelIdentifier, nameof(ModelIdentifier));
            AuthHeaderName = Required(authHeaderName, nameof(AuthHeaderName));
            AuthPrefix = authPrefix ?? string.Empty;
            ExtraHeadersJson = extraHeadersJson;
            RequestTemplateJson = requestTemplateJson;
            ResponseTextPath = Required(responseTextPath, nameof(ResponseTextPath));
            MaxInputTokens = maxInputTokens;
            MaxOutputTokens = maxOutputTokens;
            ChunkOverlapTokens = chunkOverlapTokens;
            DefaultTemperature = defaultTemperature;
        }

        // The service encrypts; the entity only stores the cipher text and the last 4 characters.
        public void SetApiKey(string encryptedKey, string? hint)
        {
            ApiKeyEncrypted = Required(encryptedKey, nameof(ApiKeyEncrypted));
            ApiKeyHint = hint;
        }

        //----Privates---------------------------------------------------------

        private static string Required(string? value, string field)
            => string.IsNullOrWhiteSpace(value)
                ? throw new ArgumentException($"{field} is required.", field)
                : value.Trim();

        private static void EnsureJsonObject(string json, string field)
        {
            try
            {
                if (JsonNode.Parse(json) is JsonObject) return;
            }
            catch (JsonException) { }

            throw new ArgumentException($"{field} must be a JSON object.", field);
        }

        private static void EnsureHeaders(string json)
        {
            try
            {
                if (JsonSerializer.Deserialize<Dictionary<string, string>>(json) is not null) return;
            }
            catch (JsonException) { }

            throw new ArgumentException("ExtraHeadersJson must be a JSON object of text values.", nameof(ExtraHeadersJson));
        }
    }
}