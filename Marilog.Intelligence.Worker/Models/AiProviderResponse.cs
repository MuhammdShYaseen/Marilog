namespace Marilog.Intelligence.Worker.Models
{
    public enum AiProviderType
    {
        OpenAI = 1,
        Anthropic = 2,
        AzureOpenAI = 3,
        GoogleGemini = 4,
        Ollama = 5,
        Embadding = 6,
    }
    public sealed class AiProviderResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public AiProviderType ProviderType { get; set; }
        public string RequestTemplateJson { get; set; } = default!;
        public string BaseUrl { get; set; } = default!;
        public string ModelIdentifier { get; set; } = default!;
        public string? ApiVersion { get; set; }
        public string AuthHeader { get; set; } = null!;
        public int MaxInputTokens { get; set; }
        public int MaxOutputTokens { get; set; }
        public int ChunkOverlapTokens { get; set; }

        public decimal DefaultTemperature { get; set; }

        public string ApiKeyName { get; set; } = default!;
        public string ApiKeyEncrypted { get; set; } = default!;
        public bool SupportsStreaming { get; set; }
    }
}
