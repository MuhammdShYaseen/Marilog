using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Kernel.Enums;

namespace Marilog.Contracts.Interfaces.Services.SystemServices
{
    public interface IStoredFileQueryService
    {
        Task<StoredFileResponse?> GetByIdAsync(int id);
        Task<IReadOnlyList<StoredFileResponse>> GetByEntityIdAsync(int entityId, EntityType entityType);
        Task<PagedResponse<StoredFileResponse>> FullTextSearchAsync(string query, int page, int pageSize, EntityType? entityType = null);
        Task<IReadOnlyList<DocumentResponse>> GetByTagsAsync(List<string> tags, CancellationToken ct = default);
        Task AddTagAsync(int documentId, string name, string color, CancellationToken ct = default);
        Task RemoveTagAsync(int documentId, int tagId, CancellationToken ct = default);
        Task<Stream> GetFileStreamAsync(int id);
    }
}
