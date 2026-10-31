

using Marilog.Kernel.Enums;

namespace Marilog.Application.Interfaces.Services.Domain
{
    public interface IDocumentNumberService
    {
        Task<string> GenerateAsync(EntityType entityType, Guid entityId, string code, string? description, CancellationToken ct = default);
        Task<string> CreateStartAsync(EntityType entityType, Guid entityId, string code,int year, int start, string? description, CancellationToken ct = default);
        Task<string> UpdateStartAsync(Guid id, int start, string? description,CancellationToken ct = default);
    }
}
