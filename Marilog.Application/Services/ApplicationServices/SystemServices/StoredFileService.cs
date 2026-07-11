
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
        private readonly IFileStorageProvider _storage;

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

        public StoredFileService(IRepository<StoredFile> repository, IFileStorageProvider storage)
        {
            _repoStoredFile = repository;
            _storage = storage;
        }

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

        public async Task<PagedResponse<StoredFileResponse>> FullTextSearchAsync(string query, int page, int pageSize, EntityType entityType, CancellationToken ct = default)
        {
            var q = _repoStoredFile.Query()
                .AsNoTracking()
                .Include(f => f.Tags)
                .Where(f =>
                    EF.Functions.Like(f.OriginalFileName, $"%{query}%") ||
                    EF.Functions.Like(f.Content!, $"%{query}%"));

                q = q.Where(f => f.EntityType == entityType);

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

        public async Task<IReadOnlyList<StoredFileResponse>> GetByTagsAsync(
            IReadOnlyList<string> tags,
            CancellationToken ct = default)
        {
            return await _repoStoredFile.Query()
                .AsNoTracking()
                .Include(f => f.Tags)
                .Where(f => f.Tags.Any(t => tags.Contains(t.Name)))
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

            foreach (var request in requests)
            {
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
                    entityId: request.EntityId);

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
            file.UpdateContentFromOCR(content);
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

        // ── Private Helpers ──────────────────────────────────────────────────

        private static async Task<string> ComputeChecksumAsync(Stream stream, CancellationToken ct)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hashBytes = await sha256.ComputeHashAsync(stream, ct);
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }
    }
}
