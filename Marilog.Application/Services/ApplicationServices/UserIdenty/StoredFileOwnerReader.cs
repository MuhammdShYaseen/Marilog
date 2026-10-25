using Marilog.Contracts.Interfaces.Services.UserIdenty;
using Marilog.Domain.Entities.SystemEntities;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Kernel.Enums;
using Microsoft.EntityFrameworkCore;

namespace Marilog.Application.Services.ApplicationServices.UserIdenty
{
    public sealed class StoredFileOwnerReader(IRepository<StoredFile> files, IRepository<StoredFolder> folders) : IStoredFileOwnerReader
    {
        public async Task<EntityType?> GetFileOwnerAsync(int fileId, CancellationToken ct = default)
            => await files.Query()
                .AsNoTracking()
                .Where(f => f.Id == fileId)
                .Select(f => (EntityType?)f.EntityType)
                .FirstOrDefaultAsync(ct);

        public async Task<IReadOnlyList<EntityType>> GetFileOwnersAsync(IReadOnlyCollection<int> fileIds, CancellationToken ct = default)
            => await files.Query()
                .AsNoTracking()
                .Where(f => fileIds.Contains(f.Id))
                .Select(f => f.EntityType)
                .Distinct()
                .ToListAsync(ct);

        public async Task<EntityType?> GetFolderOwnerAsync(int folderId, CancellationToken ct = default)
            => await folders.Query()
                .AsNoTracking()
                .Where(f => f.Id == folderId)
                .Select(f => (EntityType?)f.EntityType)
                .FirstOrDefaultAsync(ct);
    }
}