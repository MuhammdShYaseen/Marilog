using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.NotificationDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.NotifiServices;
using Marilog.Infrastructure.Helpers.UserIdentity;
using Marilog.Infrastructure.Models.Identity;
using Marilog.Kernel.Enumerations;
using Marilog.Kernel.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers.NotifiControllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _service;
        private readonly IAuthorizationService _authorization;
        private readonly UserManager<User> _userManager;
        private readonly UserPermissionReader _permissionReader;

        public NotificationsController(INotificationService service, IAuthorizationService authorization,
            UserManager<User> userManager, UserPermissionReader permissionReader)
        {
            _service = service;
            _authorization = authorization;
            _userManager = userManager;
            _permissionReader = permissionReader;
        }

        // ── Notifications ────────────────────────────────────────

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<NotificationResponse>>>> GetMine(CancellationToken ct)
        {
            var result = await _service.GetMineAsync(await ReadableTypesAsync(ct), ct);
            return Ok(ApiResponse<IReadOnlyList<NotificationResponse>>.Ok(result));
        }

        [HttpGet("unread-count")]
        public async Task<ActionResult<ApiResponse<int>>> GetUnreadCount(CancellationToken ct)
        {
            var count = await _service.GetUnreadCountAsync(await ReadableTypesAsync(ct), ct);
            return Ok(ApiResponse<int>.Ok(count));
        }

        [HttpPost("{id:long}/mark-read")]
        public async Task<IActionResult> MarkRead(long id, CancellationToken ct)
        {
            await _service.MarkReadAsync(id, ct);
            return NoContent();
        }

        [HttpPost("mark-all-read")]
        public async Task<IActionResult> MarkAllRead(CancellationToken ct)
        {
            await _service.MarkAllReadAsync(ct);
            return NoContent();
        }

        [HttpPost("records/{type}/{id:int}/mark-read")]
        public async Task<IActionResult> MarkRecordRead(EntityType type, int id, CancellationToken ct)
        {
            await _service.MarkRecordReadAsync(type, id, ct);
            return NoContent();
        }

        // ── My follows ───────────────────────────────────────────

        [HttpGet("watches")]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<WatchResponse>>>> GetMyWatches(CancellationToken ct)
        {
            var result = await _service.GetMyWatchesAsync(await ReadableTypesAsync(ct), ct);
            return Ok(ApiResponse<IReadOnlyList<WatchResponse>>.Ok(result));
        }

        [HttpGet("records/{type}/{id:int}")]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<NotificationResponse>>>> GetHistory(EntityType type, int id, CancellationToken ct)
        {
            if (await CheckReadAsync(type) is { } denied) return denied;

            var result = await _service.GetHistoryAsync(type, id, ct);
            return Ok(ApiResponse<IReadOnlyList<NotificationResponse>>.Ok(result));
        }

        // ── Follow ───────────────────────────────────────────────

        [HttpGet("watch/{type}/{id:int}")]
        public async Task<ActionResult<ApiResponse<bool>>> IsFollowing(EntityType type, int id, CancellationToken ct)
        {
            var following = await _service.IsFollowingAsync(type, id, ct);
            return Ok(ApiResponse<bool>.Ok(following));
        }

        [HttpPost("watch")]
        public async Task<ActionResult<ApiResponse<bool>>> Follow([FromBody] FollowRequest request, CancellationToken ct)
        {
            if (await CheckReadAsync(request.EntityType) is { } denied) return denied;

            var found = await _service.FollowAsync(request, ct);
            return found
                ? Ok(ApiResponse<bool>.Ok(true))
                : NotFound(ApiResponse<bool>.Fail($"{request.EntityType} {request.EntityId} was not found."));
        }

        [HttpDelete("watch/{type}/{id:int}")]
        public async Task<IActionResult> Unfollow(EntityType type, int id, CancellationToken ct)
        {
            await _service.UnfollowAsync(type, id, ct);
            return NoContent();
        }

        // ── Private Helpers ──────────────────────────────────────

        // Same rule as CommentsController.GetUnreadSummary
        private async Task<IReadOnlyCollection<EntityType>> ReadableTypesAsync(CancellationToken ct)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null || !user.IsActive) return [];

            var permissions = await _permissionReader.GetAsync(user, ct);
            return Enum.GetValues<EntityType>()
                .Where(t => permissions.Can(PermissionTargetKind.Entity, (int)PermissionCatalog.PermissionOwner(t), PermissionAction.Read))
                .ToList();
        }

        private async Task<ActionResult?> CheckReadAsync(EntityType type)
        {
            var result = await _authorization.AuthorizeAsync(
                User,
                HttpContext,
                new PermissionRequirement(PermissionTargetKind.Entity, (int)PermissionCatalog.PermissionOwner(type), PermissionAction.Read));

            return result.Succeeded
                ? null
                : Problem(detail: "You don't have permission to view this record.", statusCode: 403);
        }
    }
}