using Marilog.Domain.Common;
using Marilog.Kernel.Enums;

namespace Marilog.Domain.Entities.AiEntities
{
    public class AiProvider : Entity
    {
        public string Name { get; private set; } = default!;
        public AiProviderType ProviderType { get; private set; }

        public string BaseUrl { get; private set; } = default!;
        public string ModelIdentifier { get; private set; } = default!;
        public string? ApiVersion { get; private set; }

        public int MaxInputTokens { get; private set; }
        public int MaxOutputTokens { get; private set; }
        public int ChunkOverlapTokens { get; private set; }

        public decimal DefaultTemperature { get; private set; }

        public string ApiKeyName { get; private set; } = default!;
        public string ApiKeyEncrypted { get; private set; } = default!;

        public bool SupportsStreaming { get; private set; }

        private AiProvider() { }

        public static AiProvider Create(
            string name,
            AiProviderType providerType,
            string baseUrl,
            string modelIdentifier,
            int maxInputTokens = 8000,
            int maxOutputTokens = 2000,
            int chunkOverlapTokens = 200,
            string apiKeyName = "Authorization",
            string apiKeyEncrypted = "",
            string? apiVersion = null,
            decimal defaultTemperature = 0.1m,
            bool supportsStreaming = false)
        {
            return new AiProvider
            {
                Name = name,
                ProviderType = providerType,
                BaseUrl = baseUrl,
                ModelIdentifier = modelIdentifier,
                ApiVersion = apiVersion,
                MaxInputTokens = maxInputTokens,
                MaxOutputTokens = maxOutputTokens,
                ChunkOverlapTokens = chunkOverlapTokens,
                ApiKeyName = apiKeyName,
                ApiKeyEncrypted = apiKeyEncrypted,
                DefaultTemperature = defaultTemperature,
                SupportsStreaming = supportsStreaming,
                IsActive = true
            };
        }

        public void Update(
            string? name,
            string? baseUrl,
            string? modelIdentifier,
            int maxInputTokens,
            int maxOutputTokens,
            int chunkOverlapTokens,
            string? apiKeyName,
            string? apiKeyEncrypted,
            bool isActive)
        {
            Name = name ?? Name;
            BaseUrl = baseUrl ?? BaseUrl;
            ModelIdentifier = modelIdentifier ?? ModelIdentifier;
            ApiKeyName = apiKeyName ?? ApiKeyName;
            ApiKeyEncrypted = apiKeyEncrypted ?? ApiKeyEncrypted;

            MaxInputTokens = maxInputTokens;
            MaxOutputTokens = maxOutputTokens;
            ChunkOverlapTokens = chunkOverlapTokens;

            IsActive = isActive;
        }
    }
}
