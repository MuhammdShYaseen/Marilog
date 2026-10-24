using Marilog.Contracts.DTOs.Requests.UserIdenty;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.UserIdenty;
using Marilog.Infrastructure.Helpers.UserIdentity;
using Marilog.Kernel.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers.UserIdenty
{
    [ApiController]
    [Route("api/roles")]
    [Authorize(Policy = AuthPolicies.SuperAdmin)]
    public sealed class RolesController(IRoleManagementService roleService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<RoleResponse>>> GetAll(CancellationToken ct)
            => Ok(await roleService.GetAllAsync(ct));

        [HttpGet("{id:int}/permissions")]
        public async Task<ActionResult<IReadOnlyList<RolePermissionResponse>>> GetPermissions(int id, CancellationToken ct)
        {
            var permissions = await roleService.GetPermissionsAsync(id, ct);
            return permissions is null
                ? Problem(detail: "Role not found.", statusCode: StatusCodes.Status404NotFound)
                : Ok(permissions);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateRoleRequest request, CancellationToken ct)
            => ToActionResult(await roleService.CreateAsync(request, ct));

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, UpdateRoleRequest request, CancellationToken ct)
            => ToActionResult(await roleService.UpdateAsync(id, request, ct));

        [HttpPost("{id:int}/activate")]
        public async Task<IActionResult> Activate(int id, CancellationToken ct)
            => ToActionResult(await roleService.ActivateAsync(id, ct));

        [HttpPost("{id:int}/deactivate")]
        public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
            => ToActionResult(await roleService.DeactivateAsync(id, ct));

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
            => ToActionResult(await roleService.DeleteAsync(id, ct));

        [HttpPut("{id:int}/permissions")]
        public async Task<IActionResult> SetPermissions(
            int id, [FromBody] IReadOnlyList<RolePermissionRequest> permissions, CancellationToken ct)
            => ToActionResult(await roleService.SetPermissionsAsync(id, permissions, ct));

        private IActionResult ToActionResult(RoleManagementResult result) => result switch
        {
            RoleManagementResult.Success => NoContent(),
            RoleManagementResult.NotFound => Problem(
                detail: "Role not found.",
                statusCode: StatusCodes.Status404NotFound),
            RoleManagementResult.NameTaken => Problem(
                detail: "A role with this name already exists.",
                statusCode: StatusCodes.Status409Conflict),
            RoleManagementResult.InvalidName => Problem(
                detail: "Role name is required.",
                statusCode: StatusCodes.Status400BadRequest),
            RoleManagementResult.RoleInUse => Problem(
                detail: "This role is assigned to users. Remove it from them first, or deactivate it instead.",
                statusCode: StatusCodes.Status409Conflict),
            _ => Problem(
                detail: "The operation failed.",
                statusCode: StatusCodes.Status400BadRequest)
        };
    }
}