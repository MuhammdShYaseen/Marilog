using Marilog.Kernel.Enums;

namespace Marilog.Contracts.DTOs.Requests.AiProviderDTOs
{
    public class CreateAiProviderRequest
    {
        public string Name { get; set; } = default!;
        public AiProviderType ProviderType { get; set; }

        public string BaseUrl { get; set; } = default!;
        public string ModelIdentifier { get; set; } = default!;
        public string? ApiVersion { get; set; }

        public int MaxInputTokens { get; set; }
        public int MaxOutputTokens { get; set; }
        public int ChunkOverlapTokens { get; set; }

        public decimal DefaultTemperature { get; set; }

        public string ApiKeyName { get; set; } = default!;
        public string ApiKeyEncrypted { get; set; } = default!;
        public bool SupportsStreaming { get; set; }
    }
}
