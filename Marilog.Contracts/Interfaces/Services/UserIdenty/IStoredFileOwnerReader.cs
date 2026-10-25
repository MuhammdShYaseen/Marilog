using Marilog.Kernel.Enums;

namespace Marilog.Contracts.Interfaces.Services.UserIdenty
{
    // Server only: resolves which entity owns a stored file or folder, for permission checks.
    public interface IStoredFileOwnerReader
    {
        // null when the file doesn't exist.
        Task<EntityType?> GetFileOwnerAsync(int fileId, CancellationToken ct = default);

        // Distinct owners of the existing files among fileIds.
        Task<IReadOnlyList<EntityType>> GetFileOwnersAsync(IReadOnlyCollection<int> fileIds, CancellationToken ct = default);

        // null when the folder doesn't exist.
        Task<EntityType?> GetFolderOwnerAsync(int folderId, CancellationToken ct = default);
    }
}