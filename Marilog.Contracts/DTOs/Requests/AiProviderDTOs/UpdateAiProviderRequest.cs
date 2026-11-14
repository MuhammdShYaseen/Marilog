using Marilog.Kernel.Enums;

namespace Marilog.Contracts.DTOs.Requests.AiProviderDTOs
{
    // Purpose is fixed at creation. Activation goes through activate/deactivate.
    public class UpdateAiProviderRequest
    {
        public string Name { get; set; } = default!;
        public AiProviderType ProviderType { get; set; }
        public string BaseUrl { get; set; } = default!;
        public string ModelIdentifier { get; set; } = default!;
        public string AuthHeaderName { get; set; } = "Authorization";
        public string? AuthPrefix { get; set; }
        public string? ApiKey { get; set; }          // empty = keep the stored key
        public string? ExtraHeadersJson { get; set; }
        public string RequestTemplateJson { get; set; } = default!;
        public string ResponseTextPath { get; set; } = default!;
        public int MaxInputTokens { get; set; }
        public int MaxOutputTokens { get; set; }
        public int ChunkOverlapTokens { get; set; }
        public decimal DefaultTemperature { get; set; }
    }
}