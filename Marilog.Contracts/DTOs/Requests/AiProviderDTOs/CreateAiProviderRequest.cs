using Marilog.Kernel.Enums;

namespace Marilog.Contracts.DTOs.Requests.AiProviderDTOs
{
    public class CreateAiProviderRequest
    {
        public string Name { get; set; } = default!;
        public AiProviderType ProviderType { get; set; }
        public AiProviderPurpose Purpose { get; set; }
        public string BaseUrl { get; set; } = default!;
        public string ModelIdentifier { get; set; } = default!;
        public string AuthHeaderName { get; set; } = "Authorization";
        public string? AuthPrefix { get; set; }
        public string? ApiKey { get; set; }          // plain text; encrypted on save
        public string? ExtraHeadersJson { get; set; }
        public string RequestTemplateJson { get; set; } = default!;
        public string ResponseTextPath { get; set; } = default!;
        public int MaxInputTokens { get; set; } = 8000;
        public int MaxOutputTokens { get; set; } = 2000;
        public int ChunkOverlapTokens { get; set; } = 200;
        public decimal DefaultTemperature { get; set; } = 0.1m;
    }
}