using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Infrastructure.Helpers.UserIdentity;
using Marilog.Kernel.Enumerations;
using Marilog.Kernel.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers.SystemControllers
{
    // Any signed-in user; the read check is on the record the link opens.
    [ApiController]
    [Route("api/record-links")]
    [Authorize]
    public class RecordLinksController : ControllerBase
    {
        private readonly IRecordLinkService _links;
        private readonly IAuthorizationService _authorization;

        public RecordLinksController(IRecordLinkService links, IAuthorizationService authorization)
        {
            _links = links;
            _authorization = authorization;
        }

        [HttpGet("{type}/{id:int}")]
        public async Task<ActionResult<ApiResponse<RecordLinkResponse>>> Resolve(EntityType type, int id, CancellationToken ct)
        {
            var link = await _links.ResolveAsync(type, id, ct);
            if (link is null)
                return NotFound(ApiResponse<RecordLinkResponse>.Fail($"{type} {id} was not found."));

            var access = await _authorization.AuthorizeAsync(
                User,
                HttpContext,
                new PermissionRequirement(PermissionTargetKind.Entity,
                    (int)PermissionCatalog.PermissionOwner(link.EntityType), PermissionAction.Read));

            return access.Succeeded
                ? Ok(ApiResponse<RecordLinkResponse>.Ok(link))
                : Problem(detail: "You don't have permission to view this record.", statusCode: 403);
        }
    }
}