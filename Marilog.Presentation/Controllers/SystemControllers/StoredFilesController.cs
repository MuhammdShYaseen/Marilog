using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.StoregFileDTOs;
using Marilog.Contracts.DTOs.Requests.TagDtos;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Contracts.Interfaces.Services.UserIdenty;
using Marilog.Infrastructure.Helpers.UserIdentity;
using Marilog.Infrastructure.Models.Identity;
using Marilog.Kernel.Enumerations;
using Marilog.Kernel.Enums;
using Marilog.Presentation.PresentationDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace Marilog.Presentation.Controllers.SystemControllers
{
    // Files follow their owner record: each action checks the owner's EntityType at runtime.
    // Every action carries its own [Authorize], so PermissionConvention leaves it alone.
    // Owner NONE (the general library) passes only for super admin.
    [ApiController]
    [Route("api/StoredFiles")]
    public class StoredFilesController : ControllerBase
    {
        // Only these types may open inside the browser. Anything else is sent as a download,
        // so an uploaded file can never run as a web page on the API's address.
        private static readonly HashSet<string> InlineSafeTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "application/pdf", "image/png", "image/jpeg", "image/gif", "image/bmp", "image/webp", "text/plain"
        };
        private readonly IStoredFileService _service;
        private readonly IStoredFileOwnerReader _owners;
        private readonly IAuthorizationService _authorization;
        private readonly UserManager<User> _userManager;
        private readonly UserPermissionReader _permissionReader;
        public StoredFilesController(IStoredFileService service, IStoredFileOwnerReader owners, IAuthorizationService authorization, UserManager<User> userManager, UserPermissionReader permissionReader)
        {
            _service = service;
            _owners = owners;
            _authorization = authorization;
            _userManager = userManager;
            _permissionReader = permissionReader;
        }

        // ── Queries ──────────────────────────────────────────────────────────

        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            if (await CheckFileAsync(id, PermissionAction.Read, ct) is { } denied) return denied;

            var result = await _service.GetByIdAsync(id, ct);
            return result is null ?
                throw new KeyNotFoundException("object not found") :
                Ok(ApiResponse<StoredFileResponse>.Ok(result));
        }

        [Authorize]
        [HttpGet("entity/{entityType:int}/{entityId:int}")]
        public async Task<IActionResult> GetByEntity(int entityType, int entityId, CancellationToken ct)
        {
            if (await CheckOwnerAsync((EntityType)entityType, PermissionAction.Read) is { } denied) return denied;

            var result = await _service.GetByEntityIdAsync(entityId, (EntityType)entityType, ct);
            return Ok(ApiResponse<IReadOnlyList<StoredFileResponse>>.Ok(result));
        }

        [Authorize]
        [HttpGet("search")]
        public async Task<IActionResult> FullTextSearch([FromQuery] string query, [FromQuery] int page,
                                                        [FromQuery] int pageSize, CancellationToken ct)
        {
            var readable = await GetReadableEntityTypesAsync(ct);
            if (readable.Count == 0)
                return Problem(detail: "You don't have read permission on any record type.", statusCode: 403);
            var result = await _service.FullTextSearchAsync(query, page, pageSize, readable, ct);
            return Ok(ApiResponse<PagedResponse<StoredFileResponse>>.Ok(result));
        }

        [Authorize]
        [HttpGet("by-tags")]
        public async Task<IActionResult> GetByTags([FromQuery] List<string> tags, CancellationToken ct)
        {
            var readable = await GetReadableEntityTypesAsync(ct);
            if (readable.Count == 0)
                return Problem(detail: "You don't have read permission on any record type.", statusCode: 403);
            var result = await _service.GetByTagsAsync(tags, readable, ct);
            return Ok(ApiResponse<IReadOnlyList<StoredFileResponse>>.Ok(result));
        }

        [Authorize]
        [HttpGet("{id:int}/stream")]
        public async Task<IActionResult> GetFileStream(int id, CancellationToken ct, [FromQuery] bool inline = false)
        {
            if (await CheckFileAsync(id, PermissionAction.Read, ct) is { } denied) return denied;

            var stream = await _service.GetFileStreamAsync(id, ct);
            var file = await _service.GetByIdAsync(id, ct);

            if (file is null)
                return NotFound();

            Response.Headers.XContentTypeOptions = "nosniff";
            var isSafe = InlineSafeTypes.Contains(file.ContentType);

            if (inline && isSafe)
            {
                Response.Headers.ContentDisposition =
                    new ContentDispositionHeaderValue("inline") { FileNameStar = file.OriginalFileName }.ToString();
                return File(stream, file.ContentType);
            }

            // Word/Excel/PowerPoint: show the HTML copy the OCR job builds next to the file.
            if (inline && await _service.GetPreviewStreamAsync(id, ct) is { } preview)
            {
                await stream.DisposeAsync();
                // Marilog builds this page with every value HTML-encoded; the sandbox blocks scripts as a second guard.
                Response.Headers.ContentSecurityPolicy = "sandbox; default-src 'none'; style-src 'unsafe-inline'";
                return File(preview, "text/html; charset=utf-8");
            }

            // Inline request with nothing to show (old file, still processing, or a type with no preview):
            // say so inside the panel instead of starting a download.
            if (inline)
            {
                await stream.DisposeAsync();
                Response.Headers.ContentSecurityPolicy = "sandbox; default-src 'none'; style-src 'unsafe-inline'";
                return Content(
                    "<p style=\"font-family:sans-serif;color:#666;margin:16px\">No preview yet. " +
                    "The file may still be processing, or it was uploaded before previews existed. Use Download.</p>",
                    "text/html; charset=utf-8");
            }
            return File(stream, isSafe ? file.ContentType : "application/octet-stream", file.OriginalFileName);
        }

        [Authorize]
        [HttpGet("{id:int}/thumbnailStream")]
        public async Task<IActionResult> GetFileThumbnailStream(int id, CancellationToken ct)
        {
            if (await CheckFileAsync(id, PermissionAction.Read, ct) is { } denied) return denied;

            var stream = await _service.GetThumbnailStreamAsync(id, ct);
            return stream is null ? NotFound() : File(stream, "image/png");
        }

        // POST api/StoredFiles/reprocess-old  ← super admin; one-off: text + preview for files uploaded before previews existed
        [Authorize(Policy = AuthPolicies.SuperAdmin)]
        [HttpPost("reprocess-old")]
        public async Task<IActionResult> ReprocessOld(CancellationToken ct)
            => Ok(ApiResponse<int>.Ok(await _service.ReprocessOldFilesAsync(ct)));

        // ── Commands ─────────────────────────────────────────────────────────

        [Authorize]
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload([FromForm] FileUploadDto uploadDto, CancellationToken ct)
        {
            if (await CheckOwnerAsync(uploadDto.EntityType, PermissionAction.Create) is { } denied) return denied;

            if (uploadDto.Files is null || uploadDto.Files.Count == 0)
                return BadRequest("At least one file is required.");

            if (uploadDto.Files.Count > 20)
                return BadRequest("At 20 file is maximum to upload.");

            var requests = uploadDto.Files.Select(file => new UploadFileRequest
            {
                FileStream = file.OpenReadStream(),
                FileName = file.FileName,
                ContentType = file.ContentType,
                Size = file.Length,
                EntityType = uploadDto.EntityType,
                EntityId = uploadDto.EntityId,
                FolderId = uploadDto.FolderId
            });

            var results = await _service.UploadAsync(requests, ct);
            return Ok(results);
        }

        [Authorize]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            if (await CheckFileAsync(id, PermissionAction.Delete, ct) is { } denied) return denied;

            await _service.DeleteAsync(id, ct);
            return NoContent();
        }

        [Authorize(Policy = AuthPolicies.SuperAdmin)]
        [HttpPut("{id:int}/entity-link")]
        public async Task<IActionResult> UpdateEntityLink(
            int id,
            [FromBody] UpdateEntityLinkRequest request,
            CancellationToken ct)
        {
            await _service.UpdateEntityLinkAsync(id, request.EntityType, request.EntityId, ct);
            return NoContent();
        }

       
        [Authorize]
        [HttpPut("{id:int}/user-content")]
        public async Task<IActionResult> UpdateContent(int id, [FromBody] string content, CancellationToken ct)
        {
            if (await CheckFileAsync(id, PermissionAction.Update, ct) is { } denied) return denied;

            await _service.UpdateContentFromUserAsync(id, content, ct);

            return NoContent();
        }

        [Authorize]
        [HttpPut("{id:int}/rename")]
        public async Task<IActionResult> Rename(int id, [FromBody] string newName, CancellationToken ct)
        {
            if (await CheckFileAsync(id, PermissionAction.Update, ct) is { } denied) return denied;

            await _service.RenameAsync(id, newName, ct);

            return NoContent();
        }


        // ── Tags ─────────────────────────────────────────────────────────────

        [Authorize]
        [HttpPost("{storedFileId:int}/tags")]
        public async Task<IActionResult> AddTag(
            int storedFileId,
            [FromBody] AddTagRequest request,
            CancellationToken ct)
        {
            if (await CheckFileAsync(storedFileId, PermissionAction.Update, ct) is { } denied) return denied;

            await _service.AddTagAsync(storedFileId, request.Name, request.Color, ct);
            return NoContent();
        }

        [Authorize]
        [HttpDelete("{storedFileId:int}/tags/{tagId:int}")]
        public async Task<IActionResult> RemoveTag(int storedFileId, int tagId, CancellationToken ct)
        {
            if (await CheckFileAsync(storedFileId, PermissionAction.Update, ct) is { } denied) return denied;

            await _service.RemoveTagAsync(storedFileId, tagId, ct);
            return NoContent();
        }

        // ── Folders ──────────────────────────────────────────────────────────

        [Authorize]
        [HttpGet("folders/entity/{entityType:int}/{entityId:int}")]
        public async Task<IActionResult> GetFoldersByEntity(int entityType, int entityId, CancellationToken ct)
        {
            if (await CheckOwnerAsync((EntityType)entityType, PermissionAction.Read) is { } denied) return denied;

            var result = await _service.GetFoldersByEntityIdAsync(entityId, (EntityType)entityType, ct);
            return Ok(ApiResponse<IReadOnlyList<StoredFolderResponse>>.Ok(result));
        }

        [Authorize]
        [HttpPost("folders")]
        public async Task<IActionResult> CreateFolder([FromBody] CreateStoredFolderRequest request, CancellationToken ct)
        {
            if (await CheckOwnerAsync(request.EntityType, PermissionAction.Create) is { } denied) return denied;

            var result = await _service.CreateFolderAsync(request, ct);
            return Ok(ApiResponse<StoredFolderResponse>.Ok(result));
        }

        [Authorize]
        [HttpPut("folders/{id:int}/name")]
        public async Task<IActionResult> RenameFolder(int id, [FromBody] RenameStoredFolderRequest request, CancellationToken ct)
        {
            if (await CheckFolderAsync(id, PermissionAction.Update, ct) is { } denied) return denied;

            await _service.RenameFolderAsync(id, request.Name, ct);
            return NoContent();
        }

        [Authorize]
        [HttpPut("folders/{id:int}/parent")]
        public async Task<IActionResult> MoveFolder(int id, [FromBody] MoveStoredFolderRequest request, CancellationToken ct)
        {
            if (await CheckFolderAsync(id, PermissionAction.Update, ct) is { } denied) return denied;

            await _service.MoveFolderAsync(id, request.TargetParentFolderId, ct);
            return NoContent();
        }

        [Authorize]
        [HttpDelete("folders/{id:int}")]
        public async Task<IActionResult> DeleteFolder(int id, CancellationToken ct)
        {
            if (await CheckFolderAsync(id, PermissionAction.Delete, ct) is { } denied) return denied;

            await _service.DeleteFolderAsync(id, ct);
            return NoContent();
        }

        [Authorize]
        [HttpPost("move")]
        public async Task<IActionResult> MoveFiles([FromBody] MoveStoredFilesRequest request, CancellationToken ct)
        {
            foreach (var owner in await _owners.GetFileOwnersAsync(request.FileIds, ct))
            {
                if (await CheckOwnerAsync(owner, PermissionAction.Update) is { } denied) return denied;
            }

            await _service.MoveFilesAsync(request, ct);
            return NoContent();
        }

        // ── Owner permission checks ──────────────────────────────────────────
        // Each returns null when allowed, otherwise the result the action should return.

        private async Task<IActionResult?> CheckOwnerAsync(EntityType owner, PermissionAction action)
        {
            var result = await _authorization.AuthorizeAsync(
                User,
                HttpContext,
                new PermissionRequirement(PermissionTargetKind.Entity, (int)PermissionCatalog.PermissionOwner(owner), action));

            return result.Succeeded
                ? null
                : Problem(
                    detail: $"You don't have permission to {action.ToString().ToLowerInvariant()} files of this record.",
                    statusCode: 403);
        }

        private async Task<IActionResult?> CheckFileAsync(int fileId, PermissionAction action, CancellationToken ct)
        {
            var owner = await _owners.GetFileOwnerAsync(fileId, ct);
            return owner is null ? NotFound() : await CheckOwnerAsync(owner.Value, action);
        }

        private async Task<IActionResult?> CheckFolderAsync(int folderId, PermissionAction action, CancellationToken ct)
        {
            var owner = await _owners.GetFolderOwnerAsync(folderId, ct);
            return owner is null ? NotFound() : await CheckOwnerAsync(owner.Value, action);
        }

        private async Task<List<EntityType>> GetReadableEntityTypesAsync(CancellationToken ct)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null || !user.IsActive) return [];

            var permissions = await _permissionReader.GetAsync(user, ct);

            return Enum.GetValues<EntityType>()
                .Where(t => permissions.Can(PermissionTargetKind.Entity, (int)PermissionCatalog.PermissionOwner(t), PermissionAction.Read))
                .ToList();
        }
    }
}