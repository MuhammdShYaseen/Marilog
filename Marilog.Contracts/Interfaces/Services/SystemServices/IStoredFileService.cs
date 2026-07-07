using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.StoregFileDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Kernel.Enums;

namespace Marilog.Contracts.Interfaces.Services.SystemServices
{
    public interface IStoredFileService
    {
        // Queries
        Task<StoredFileResponse?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<StoredFileResponse>> GetByEntityIdAsync(int entityId, EntityType entityType, CancellationToken ct = default);
        Task<PagedResponse<StoredFileResponse>> FullTextSearchAsync(string query, int page, int pageSize, EntityType entityType, CancellationToken ct = default);
        Task<IReadOnlyList<StoredFileResponse>> GetByTagsAsync(IReadOnlyList<string> tags, CancellationToken ct = default);
        Task<Stream> GetFileStreamAsync(int id, CancellationToken ct = default);

        // Commands
        Task<IReadOnlyList<StoredFileResponse>> UploadAsync(IEnumerable<UploadFileRequest> requests, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);
        Task UpdateEntityLinkAsync(int id, EntityType entityType, int? entityId, CancellationToken ct = default);
        Task UpdateContentFromOCRAsync(Guid id, string content, CancellationToken ct = default);

        // Tags
        Task AddTagAsync(int storedFileId, string name, string color, CancellationToken ct = default);
        Task RemoveTagAsync(int storedFileId, int tagId, CancellationToken ct = default);
    }
}
