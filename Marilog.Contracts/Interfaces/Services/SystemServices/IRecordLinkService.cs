using Marilog.Contracts.DTOs.Responses;
using Marilog.Kernel.Enums;

namespace Marilog.Contracts.Interfaces.Services.SystemServices
{
    public interface IRecordLinkService
    {
        // null = the record doesn't exist, or its type has no page.
        Task<RecordLinkResponse?> ResolveAsync(EntityType type, int id, CancellationToken ct = default);
    }
}