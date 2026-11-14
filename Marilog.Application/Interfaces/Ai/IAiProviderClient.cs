using Marilog.Domain.Entities.AiEntities;

namespace Marilog.Application.Interfaces.Ai
{
    // Transport only: posts a built body and returns the value at the provider's ResponseTextPath.
    public interface IAiProviderClient
    {
        Task<string> SendAsync(AiProvider provider, string? apiKey, string body, CancellationToken ct);
    }
}