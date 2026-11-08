using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.CommentDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Infrastructure.Helpers.UserIdentity;
using Marilog.Kernel.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Marilog.Presentation.Controllers.SystemControllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _service;
        private readonly IAuthorizationService _authorization;
        public CommentsController(ICommentService service, IAuthorizationService authorizationService)
        {
            _service = service;
            _authorization = authorizationService;
        }

        // ── Queries ──────────────────────────────────────────────

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<CommentResponse>>> GetById(
            int id,
            CancellationToken ct)
        {


            var comment = await _service.GetByIdAsync(id, ct);
            if (comment is null)
                return NotFound(ApiResponse<CommentResponse>.Fail(
                    $"Comment with ID {id} was not found."));

            if (await CheckReadAsync(comment.EntityType) is { } denied) return denied;

            return Ok(ApiResponse<CommentResponse>.Ok(comment));
        }

        [Authorize(Policy = AuthPolicies.SuperAdmin)]

        [HttpGet("user/{userId:guid}")]
        public async Task<ActionResult<ApiResponse<PagedResponse<CommentResponse>>>> GetByUserId(
            Guid userId,
            [FromQuery] PagedRequest request,
            CancellationToken ct)
        {
            var result = await _service.GetByUserIdAsync(userId, request, ct);

            return Ok(ApiResponse<PagedResponse<CommentResponse>>.Ok(result));
        }

        [HttpGet("entity/{entityType}/{entityId:int}")]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<CommentResponse>>>> GetByEntity(
            EntityType entityType,
            int entityId,
            CancellationToken ct)
        {
            if (await CheckReadAsync(entityType) is { } denied) return denied;

            var result = await _service.GetByEntityAsync(entityType, entityId, ct);

            return Ok(ApiResponse<IReadOnlyList<CommentResponse>>.Ok(result));
        }

        [HttpGet]
        [Authorize(Policy = AuthPolicies.SuperAdmin)]
        public async Task<ActionResult<ApiResponse<PagedResponse<CommentResponse>>>> GetAll(
            [FromQuery] PagedRequest request,
            CancellationToken ct)
        {
            var result = await _service.GetAllAsync(request, ct);

            return Ok(ApiResponse<PagedResponse<CommentResponse>>.Ok(result));
        }

        // ── Commands ─────────────────────────────────────────────

        [HttpPost]
        public async Task<ActionResult<ApiResponse<CommentResponse>>> Create(
            [FromBody] CreateCommentRequest request,
            CancellationToken ct)
        {
            if (await CheckReadAsync(request.EntityType) is { } denied) return denied;

            var result = await _service.CreateAsync(request, ct);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Id },
                ApiResponse<CommentResponse>.Ok(result, "Comment created successfully."));
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse<CommentResponse>>> Update(
            int id,
            [FromBody] UpdateCommentRequest request,
            CancellationToken ct)
        {
            var result = await _service.UpdateAsync(id, request, ct);

            return Ok(ApiResponse<CommentResponse>.Ok(
                result,
                "Comment updated successfully."));
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(
            int id,
            CancellationToken ct)
        {
            await _service.DeleteAsync(id, ct);

            return Ok(ApiResponse<bool>.Ok(
                true,
                "Comment deleted successfully."));
        }

        // ── Private Helpers ──────────────────────────────────────
        private async Task<ActionResult?> CheckReadAsync(EntityType owner)
        {
            var result = await _authorization.AuthorizeAsync(
                User,
                HttpContext,
                new PermissionRequirement(PermissionTargetKind.Entity, (int)owner, PermissionAction.Read));

            return result.Succeeded
                ? null
                : Problem(detail: "You don't have permission to view this record's comments.", statusCode: 403);
        }
    }
}