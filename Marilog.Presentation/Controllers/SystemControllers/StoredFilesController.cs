
using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.StoregFileDTOs;
using Marilog.Contracts.DTOs.Requests.TagDtos;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Domain.Entities.SystemEntities;
using Marilog.Kernel.Enums;
using Marilog.Presentation.Controllers.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers.SystemControllers
{
    [ApiController]
    [Route("api/StoredFiles")]
    public class StoredFilesController : ControllerBase
    {
        private readonly IStoredFileService _service;

        public StoredFilesController(IStoredFileService service)
        {
            _service = service;
        }

        // ── Queries ──────────────────────────────────────────────────────────

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var result = await _service.GetByIdAsync(id, ct);
            return result is null ? 
                throw new KeyNotFoundException("object not found") : 
                Ok(ApiResponse<StoredFileResponse>.Ok(result));
        }

        [HttpGet("entity/{entityType:int}/{entityId:int}")]
        public async Task<IActionResult> GetByEntity(int entityType, int entityId, CancellationToken ct)
        {
            var result = await _service.GetByEntityIdAsync(entityId, (EntityType)entityType, ct);
            return Ok(ApiResponse<IReadOnlyList<StoredFileResponse>>.Ok(result));
        }

        [HttpGet("search")]
        public async Task<IActionResult> FullTextSearch(
            [FromQuery] string query,
            [FromQuery] int page,
            [FromQuery] int pageSize,
            [FromQuery] EntityType entityType,
            CancellationToken ct)
        {
            var result = await _service.FullTextSearchAsync(query, page, pageSize, entityType, ct);
            return Ok(ApiResponse<PagedResponse<StoredFileResponse>>.Ok(result));
        }

        [HttpGet("by-tags")]
        public async Task<IActionResult> GetByTags([FromQuery] List<string> tags, CancellationToken ct)
        {
            var result = await _service.GetByTagsAsync(tags, ct);
            return Ok(ApiResponse<IReadOnlyList<StoredFileResponse>>.Ok(result));
        }

        [HttpGet("{id:int}/stream")]
        public async Task<IActionResult> GetFileStream(int id, CancellationToken ct)
        {
            var stream = await _service.GetFileStreamAsync(id, ct);
            var file = await _service.GetByIdAsync(id, ct);

            return File(stream, file!.ContentType, file.OriginalFileName);
        }

        // ── Commands ─────────────────────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> Upload([FromForm] IFormFile file, [FromForm] EntityType entityType, [FromForm] int? entityId, CancellationToken ct)
        {
            var request = new UploadFileRequest
            {
                FileStream = file.OpenReadStream(),
                FileName = file.FileName,
                ContentType = file.ContentType,
                Size = file.Length,
                EntityType = entityType,
                EntityId = entityId
            };

            var result = await _service.UploadAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            await _service.DeleteAsync(id, ct);
            return NoContent();
        }

        [HttpPut("{id:int}/entity-link")]
        public async Task<IActionResult> UpdateEntityLink(
            int id,
            [FromBody] UpdateEntityLinkRequest request,
            CancellationToken ct)
        {
            await _service.UpdateEntityLinkAsync(id, request.EntityType, request.EntityId, ct);
            return NoContent();
        }

        [HttpPut("{id:int}/ocr-content")]
        [InternalApiKey]
        public async Task<IActionResult> UpdateOcrContent(int id, [FromBody] UpdateOcrContentRequest request, CancellationToken ct)
        {
            await _service.UpdateContentFromOCRAsync(id, request.Content, ct);
            return NoContent();
        }

        // ── Tags ─────────────────────────────────────────────────────────────

        [HttpPost("{storedFileId:int}/tags")]
        public async Task<IActionResult> AddTag(
            int storedFileId,
            [FromBody] AddTagRequest request,
            CancellationToken ct)
        {
            await _service.AddTagAsync(storedFileId, request.Name, request.Color, ct);
            return NoContent();
        }

        [HttpDelete("{storedFileId:int}/tags/{tagId:int}")]
        public async Task<IActionResult> RemoveTag(int storedFileId, int tagId, CancellationToken ct)
        {
            await _service.RemoveTagAsync(storedFileId, tagId, ct);
            return NoContent();
        }
    }
}