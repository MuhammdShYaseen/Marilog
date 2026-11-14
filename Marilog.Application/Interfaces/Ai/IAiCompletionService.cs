using Marilog.Application.DTOs.Ai;
using Marilog.Kernel.Enums;

namespace Marilog.Application.Interfaces.Ai
{
    // Entry point for any AI feature. A feature asks for a purpose, never a vendor.
    public interface IAiCompletionService
    {
        // The active provider for the purpose.
        Task<string> CompleteAsync(AiProviderPurpose purpose, AiRequestContext context, CancellationToken ct);

        // One provider by id, active or not (testing before activation).
        Task<string> CompleteAsync(int providerId, AiRequestContext context, CancellationToken ct);
    }
}