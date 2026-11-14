using Marilog.Kernel.Enums;

namespace Marilog.Contracts.DTOs.Responses
{
    // The API key never leaves the server: only HasApiKey and its last 4 characters.
    public sealed class AiProviderResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public AiProviderType ProviderType { get; set; }
        public AiProviderPurpose Purpose { get; set; }
        public string BaseUrl { get; set; } = default!;
        public string ModelIdentifier { get; set; } = default!;
        public string AuthHeaderName { get; set; } = default!;
        public string AuthPrefix { get; set; } = string.Empty;
        public bool HasApiKey { get; set; }
        public string? ApiKeyHint { get; set; }
        public string? ExtraHeadersJson { get; set; }
        public string RequestTemplateJson { get; set; } = default!;
        public string ResponseTextPath { get; set; } = default!;
        public int MaxInputTokens { get; set; }
        public int MaxOutputTokens { get; set; }
        public int ChunkOverlapTokens { get; set; }
        public decimal DefaultTemperature { get; set; }
        public bool IsActive { get; set; }
    }
}