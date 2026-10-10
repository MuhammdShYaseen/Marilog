
using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.StoregFileDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.Infrastructure;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Domain.Entities.SystemEntities;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Kernel.Enums;
using Microsoft.EntityFrameworkCore;

using System.Linq.Expressions;


namespace Marilog.Application.Services.ApplicationServices.SystemServices
{
    public class StoredFileService : IStoredFileService
    {
        private readonly IRepository<StoredFile> _repoStoredFile;
        private readonly IRepository<StoredFolder> _repoStoredFolder;
        private readonly IFileStorageProvider _storage;

        public StoredFileService(IRepository<StoredFile> repository, IRepository<StoredFolder> folderRepository, IFileStorageProvider storage)
        {
            _repoStoredFile = repository;
            _repoStoredFolder = folderRepository;
            _storage = storage;
        }
        // ── Mapping ─────────────────────────────────────────────────────────
        private static readonly Expression<Func<StoredFile, StoredFileResponse>> ToResponse = f => new StoredFileResponse
        {
            Id = f.Id,
            OriginalFileName = f.OriginalFileName,
            StoredFileName = f.StoredFileName,
            RelativePath = f.RelativePath,
            ContentType = f.ContentType,
            Size = f.Size,
            Checksum = f.Checksum,
            EntityType = f.EntityType,
            EntityId = f.EntityId,
            FolderId = f.FolderId,
            CreatedAt = f.CreatedAt,
            Content = f.Content,
            HasThumbnail = f.ThumbnailRelativePath != null,
            Tags = f.Tags.Select(t => new TagResponse
            {
                Id = t.Id,
                Name = t.Name,
                Color = t.Color
            }).ToList()
        };

        private static readonly Expression<Func<StoredFolder, StoredFolderResponse>> ToFolderResponse = f => new StoredFolderResponse
        {
            Id = f.Id,
            Name = f.Name,
            ParentFolderId = f.ParentFolderId
        };

        // ── Queries ──────────────────────────────────────────────────────────

        public async Task<StoredFileResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _repoStoredFile.Query()
                .AsNoTracking()
                .Include(f => f.Tags)
                .Where(f => f.Id == id)
                .Select(ToResponse)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<IReadOnlyList<StoredFileResponse>> GetByEntityIdAsync(int entityId, EntityType entityType,  CancellationToken ct = default)
        {
            return await _repoStoredFile.Query()
                .AsNoTracking()
                .Include(f => f.Tags)
                .Where(f => f.EntityId == entityId && f.EntityType == entityType)
                .Select(ToResponse)
                .ToListAsync(ct);
        }

        public async Task<PagedResponse<StoredFileResponse>> FullTextSearchAsync(string query, int page, int pageSize, EntityType? entityType = null, CancellationToken ct = default)
        {
            var q = _repoStoredFile.Query()
                .AsNoTracking()
                .Include(f => f.Tags)
                .Where(f =>
                    EF.Functions.Like(f.OriginalFileName, $"%{query}%") ||
                    EF.Functions.Like(f.Content!, $"%{query}%"));

            if (entityType.HasValue && entityType.Value != 0)
            {
                q = q.Where(f => f.EntityType == entityType.Value);
            }

            var total = await q.CountAsync(ct);

            var items = await q
                .OrderByDescending(f => f.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(ToResponse)
                .ToListAsync(ct);

            return new PagedResponse<StoredFileResponse>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<IReadOnlyList<StoredFileResponse>> GetByTagsAsync(IReadOnlyList<string> tags, CancellationToken ct = default)
        {
            var searchTerms = tags
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct()
                .ToList();

            if (searchTerms.Count == 0)
            {
                return [];
            }

            var query = _repoStoredFile.Query()
                .AsNoTracking()
                .Include(f => f.Tags)
                .AsQueryable();

            query = query.Where(file =>
                file.Tags.Any(tag =>
                    EF.Functions.Like(tag.Name, "%" + searchTerms[0] + "%")));

            for (var i = 1; i < searchTerms.Count; i++)
            {
                var term = searchTerms[i];

                query = query.Where(file =>
                    file.Tags.Any(tag =>
                        EF.Functions.Like(tag.Name, "%" + term + "%")));
            }

            return await query
                .Select(ToResponse)
                .ToListAsync(ct);
        }

        public async Task<Stream> GetFileStreamAsync(int id, CancellationToken ct = default)
        {
            var file = await _repoStoredFile.Query()
                .AsNoTracking()
                .Where(f => f.Id == id)
                .Select(f => new { f.RelativePath })
                .FirstOrDefaultAsync(ct)
                ?? throw new ArgumentNullException(nameof(StoredFile) + id.ToString());

            return await _storage.ReadAsync(file.RelativePath, ct);
        }

        public async Task<Stream?> GetThumbnailStreamAsync(int id, CancellationToken ct = default)
        {
            var thumbnailPath = await _repoStoredFile.Query()
                .AsNoTracking()
                .Where(f => f.Id == id)
                .Select(f => f.ThumbnailRelativePath)
                .FirstOrDefaultAsync(ct);

            if (string.IsNullOrWhiteSpace(thumbnailPath))
                return null;

            return await _storage.ReadAsync(thumbnailPath, ct);
        }

        // ── Commands ─────────────────────────────────────────────────────────

        public async Task<IReadOnlyList<StoredFileResponse>> UploadAsync(IEnumerable <UploadFileRequest> requests,
            CancellationToken ct = default)
        {
            var files = new List<StoredFile>();
            var requestList = requests.ToList();

            // validate target folders before writing anything to disk
            foreach (var r in requestList.Where(r => r.FolderId.HasValue))
                EnsureSameEntity(await GetFolderOrThrowAsync(r.FolderId!.Value, ct), r.EntityType, r.EntityId);

            foreach (var request in requestList)
            {
                if (!CanConvertToPdf(request.FileName))
                    continue;


                var checksum = await ComputeChecksumAsync(request.FileStream, ct);

                // reset stream after checksum read
                request.FileStream.Position = 0;

                var storedFileName = $"{Guid.NewGuid()}{Path.GetExtension(request.FileName)}";
                var relativePath = await _storage.SaveAsync(request.FileStream, storedFileName, ct);

                var file = StoredFile.Create(
                    originalFileName: request.FileName,
                    storedFileName: storedFileName,
                    relativePath: relativePath,
                    contentType: request.ContentType,
                    size: request.Size,
                    checksum: checksum,
                    entityType: request.EntityType,
                    entityId: request.EntityId,
                    folderId: request.FolderId);

                await _repoStoredFile.AddAsync(file, ct);   // يضيف للـ context بس، من غير Save
                files.Add(file);
            }

            await _repoStoredFile.SaveChangesAsync(ct);     // Save واحدة بس لكل الملفات مع بعض

            var ids = files.Select(f => f.Id).ToList();

            return await _repoStoredFile
                .Query()
                .AsNoTracking()
                .Where(x => ids.Contains(x.Id))
                .Select(ToResponse)
                .ToListAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var file = await _repoStoredFile.GetByIdAsync(id, ct)
                ?? throw new ArgumentNullException(nameof(StoredFile) + id.ToString());

            await _storage.DeleteAsync(file.RelativePath, ct);

            _repoStoredFile.HardDelete(file);
            await _repoStoredFile.SaveChangesAsync(ct);
        }

        public async Task UpdateEntityLinkAsync(int id, EntityType entityType, int? entityId, CancellationToken ct = default)
        {
            var file = await _repoStoredFile.GetByIdAsync(id, ct)
                ?? throw new ArgumentNullException(nameof(StoredFile) + id.ToString());

            file.UpdateEntityLink(entityType, entityId);
            await _repoStoredFile.SaveChangesAsync(ct);
        }

        public async Task UpdateContentFromOCRAsync(Guid id, string content, string? thumbnailPath, CancellationToken ct = default)
        {
            var file = await _repoStoredFile.Query()
                        .IgnoreQueryFilters()
                        .FirstOrDefaultAsync(x => x.Guid == id, ct)
                ?? throw new ArgumentNullException(nameof(StoredFile)+ id.ToString());
            if(thumbnailPath != null)
            {
                file.SetThumbnail(_storage.GetRelativePath(thumbnailPath));
            }
            file.UpdateContent(content);
            await _repoStoredFile.SaveChangesAsync(ct);
        }

        public async Task UpdateContentFromUserAsync(int id, string content, CancellationToken ct = default)
        {
            var file = await _repoStoredFile.GetByIdAsync(id, ct);
            if(file == null)
                throw new KeyNotFoundException($"there is no stored file with ID : {id}");

            file.UpdateContent(content);
            _repoStoredFile.Update(file);
            await _repoStoredFile.SaveChangesAsync(ct);
        }

        // ── Tags ─────────────────────────────────────────────────────────────

        public async Task AddTagAsync(int storedFileId, string name, string color, CancellationToken ct = default)
        {
            var file = await _repoStoredFile.Query()
                .Include(f => f.Tags)
                .FirstOrDefaultAsync(f => f.Id == storedFileId, ct)
                ?? throw new ArgumentNullException(nameof(StoredFile) + storedFileId.ToString());

            file.AddTag(name, color);
            await _repoStoredFile.SaveChangesAsync(ct);
        }

        public async Task RemoveTagAsync(
            int storedFileId,
            int tagId,
            CancellationToken ct = default)
        {
            var file = await _repoStoredFile.Query()
                .Include(f => f.Tags)
                .FirstOrDefaultAsync(f => f.Id == storedFileId, ct)
                ?? throw new ArgumentNullException(nameof(StoredFile) + storedFileId.ToString());

            file.RemoveTag(tagId);
            await _repoStoredFile.SaveChangesAsync(ct);
        }


        // ── Folders ──────────────────────────────────────────────────────────

        public async Task<IReadOnlyList<StoredFolderResponse>> GetFoldersByEntityIdAsync(int entityId, EntityType entityType, CancellationToken ct = default)
        {
            return await _repoStoredFolder.Query()
                .AsNoTracking()
                .Where(f => f.EntityId == entityId && f.EntityType == entityType)
                .OrderBy(f => f.Name)
                .Select(ToFolderResponse)
                .ToListAsync(ct);
        }

        public async Task<StoredFolderResponse> CreateFolderAsync(CreateStoredFolderRequest request, CancellationToken ct = default)
        {
            if (request.ParentFolderId is int parentId)
                EnsureSameEntity(await GetFolderOrThrowAsync(parentId, ct), request.EntityType, request.EntityId);

            var folder = StoredFolder.Create(request.Name, request.ParentFolderId, request.EntityType, request.EntityId);
            await EnsureUniqueNameAsync(folder, folder.ParentFolderId, ct);

            await _repoStoredFolder.AddAsync(folder, ct);
            await _repoStoredFolder.SaveChangesAsync(ct);

            return new StoredFolderResponse { Id = folder.Id, Name = folder.Name, ParentFolderId = folder.ParentFolderId };
        }

        public async Task RenameFolderAsync(int id, string name, CancellationToken ct = default)
        {
            var folder = await GetFolderOrThrowAsync(id, ct);

            folder.Rename(name);
            await EnsureUniqueNameAsync(folder, folder.ParentFolderId, ct);

            await _repoStoredFolder.SaveChangesAsync(ct);
        }

        public async Task MoveFolderAsync(int id, int? targetParentFolderId, CancellationToken ct = default)
        {
            var folder = await GetFolderOrThrowAsync(id, ct);
            if (folder.ParentFolderId == targetParentFolderId)
                return;

            if (targetParentFolderId is int targetId)
            {
                EnsureSameEntity(await GetFolderOrThrowAsync(targetId, ct), folder.EntityType, folder.EntityId);

                // walk up from the target; if we meet the folder itself → cycle
                var parents = await _repoStoredFolder.Query()
                    .AsNoTracking()
                    .Where(f => f.EntityType == folder.EntityType && f.EntityId == folder.EntityId)
                    .ToDictionaryAsync(f => f.Id, f => f.ParentFolderId, ct);

                for (int? current = targetId; current is int c; current = parents.GetValueOrDefault(c))
                {
                    if (c == folder.Id)
                        throw new InvalidOperationException("A folder can't be moved into itself or one of its subfolders.");
                }
            }

            folder.MoveTo(targetParentFolderId);
            await EnsureUniqueNameAsync(folder, targetParentFolderId, ct);

            await _repoStoredFolder.SaveChangesAsync(ct);
        }

        public async Task DeleteFolderAsync(int id, CancellationToken ct = default)
        {
            var folder = await GetFolderOrThrowAsync(id, ct);

            var hasFiles = await _repoStoredFile.Query().AnyAsync(f => f.FolderId == id, ct);
            var hasSubfolders = await _repoStoredFolder.Query().AnyAsync(f => f.ParentFolderId == id, ct);

            if (hasFiles || hasSubfolders)
                throw new InvalidOperationException("Only empty folders can be deleted.");

            _repoStoredFolder.HardDelete(folder);
            await _repoStoredFolder.SaveChangesAsync(ct);
        }

        public async Task MoveFilesAsync(MoveStoredFilesRequest request, CancellationToken ct = default)
        {
            var files = await _repoStoredFile.Query()
                .Where(f => request.FileIds.Contains(f.Id))
                .ToListAsync(ct);

            if (files.Count == 0)
                return;

            if (request.TargetFolderId is int targetId)
            {
                var target = await GetFolderOrThrowAsync(targetId, ct);
                foreach (var file in files)
                    EnsureSameEntity(target, file.EntityType, file.EntityId);
            }

            foreach (var file in files)
                file.MoveToFolder(request.TargetFolderId);

            await _repoStoredFile.SaveChangesAsync(ct);
        }


        // ── Private Helpers ──────────────────────────────────────────────────

        private static async Task<string> ComputeChecksumAsync(Stream stream, CancellationToken ct)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hashBytes = await sha256.ComputeHashAsync(stream, ct);
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }

        private static bool CanConvertToPdf(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            return extension is
                ".pdf" or
                ".doc" or
                ".docx" or
                ".xls" or
                ".xlsx" or
                ".ppt" or
                ".pptx" or
                ".txt" or
                ".rtf" or
                ".jpg" or
                ".jpeg" or
                ".png" or
                ".bmp" or
                ".gif" or
                ".tif" or
                ".tiff";
        }


        //--Folder Helpper
        private async Task<StoredFolder> GetFolderOrThrowAsync(int id, CancellationToken ct)
        {
            return await _repoStoredFolder.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException($"there is no folder with ID : {id}");
        }

        private static void EnsureSameEntity(StoredFolder folder, EntityType entityType, int? entityId)
        {
            if (folder.EntityType != entityType || folder.EntityId != entityId)
                throw new InvalidOperationException("The folder belongs to a different record.");
        }

        private async Task EnsureUniqueNameAsync(StoredFolder folder, int? parentFolderId, CancellationToken ct)
        {
            var exists = await _repoStoredFolder.Query()
                .AnyAsync(f => f.Id != folder.Id
                            && f.EntityType == folder.EntityType
                            && f.EntityId == folder.EntityId
                            && f.ParentFolderId == parentFolderId
                            && f.Name == folder.Name, ct);

            if (exists)
                throw new InvalidOperationException($"A folder named \"{folder.Name}\" already exists here.");
        }
    }
}
